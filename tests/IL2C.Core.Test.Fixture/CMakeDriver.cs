/////////////////////////////////////////////////////////////////////////////////////////////////
//
// IL2C - A translator for ECMA-335 CIL/MSIL to C language.
// Copyright (c) 2016-2019 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IL2C
{
    internal static class CMakeDriver
    {
        public static async Task<string> BuildAsync(
            string binPath, string configuration, string sourcePath, string il2cRuntimePath)
        {
            var basePath = Path.GetDirectoryName(sourcePath);
            var outPath = Path.Combine(basePath, "build");
            var executablePath = Path.Combine(outPath, Path.GetFileNameWithoutExtension(sourcePath) + ".exe");

            // Step1: Generate ninja script by cmake.
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);

                var scriptPath = Path.Combine(basePath, "CMakeLists.txt");
                await TestUtilities.CopyResourceToTextFileAsync(scriptPath, "CMakeLists.txt");

                try
                {
                    var cmakeLog = await TestUtilities.RetryIfStrangeProblemAsync(async () =>
                    {
                        var (exitCode, log) = await TestUtilities.ExecuteAsync(
                            outPath, new[] { binPath },
                            Path.Combine(binPath, "cmake.exe"), "-G", "Ninja", "-DCMAKE_MAKE_PROGRAM=ninja.exe", "-DPLATFORM=mingw32", $"-DCONFIGURATION={configuration}", "..");
                        if (exitCode != 0)
                        {
                            throw new Exception("cmake [ExitCode=" + exitCode + "]: " + log);
                        }
                        return log;
                    });
                }
                catch
                {
                    try
                    {
                        Directory.Delete(outPath, true);
                    }
                    catch
                    {
                    }
                    throw;
                }
            }

            // Step2: Execute ninja
            var cmakeNinjaLog = await TestUtilities.RetryIfStrangeProblemAsync(async () =>
            {
                var (exitCode, log) = await TestUtilities.ExecuteAsync(
                    outPath, new[] { binPath },
                    Path.Combine(binPath, "cmake.exe"), "--build", ".", "-j");
                if (exitCode != 0)
                {
                    throw new Exception("cmake [ninja] [ExitCode=" + exitCode + "]: " + log);
                }
                return log;
            });

            // Step3: Execute native binary
            var testLog = await TestUtilities.RetryIfStrangeProblemAsync(async () =>
            {
                var (exitCode, log) = await TestUtilities.ExecuteAsync(
                    outPath, new[] { outPath }, executablePath);
                if (exitCode != 0)
                {
                    throw new Exception("test [ExitCode=" + exitCode + "]: " + log);
                }
                return log;
            });

            return testLog;
        }

        // FASTER than cmake: It's direct gcc driver with self-parsing cmake configuration.
        public static async Task<string> BuildDirectlyAsync(
            string binPath, string configuration, string sourcePath, string il2cRuntimePath)
        {
            var basePath = Path.GetDirectoryName(sourcePath);
            var outPath = Path.Combine(basePath, "build");
            var executablePath = Path.Combine(outPath, Path.GetFileNameWithoutExtension(sourcePath) + ".exe");

            var currentListDir = Path.GetFullPath(
                Path.Combine(il2cRuntimePath, "cmake"));
            var dict = await CMakeListsSimpleParser.ExtractDefinitionsAsync(
                Path.Combine(currentListDir, "gcc4-win-mingw32.cmake"),
                new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
                    { "Configuration", configuration },
                    { "Platform", "mingw32" },
                    { "CMAKE_CURRENT_LIST_DIR", currentListDir }
                });

            var incDir = dict.TryGetValue("INCDIR", out var id) ? id : string.Empty;
            var libDir = dict.TryGetValue("LIBDIR", out var ld) ? ld : string.Empty;
            var cdefs = dict.TryGetValue("COMPILE_DEFINITIONS", out var cd) ? cd : string.Empty;
            var ccflags = dict.TryGetValue("CMAKE_C_FLAGS", out var ccf) ? ccf : string.Empty;
            var libs = dict.TryGetValue("IL2C_LIBRARY_NAME_BASE", out var ls) ? $"-l{ls}" : string.Empty;

            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);

                var scriptPath = Path.Combine(basePath, "CMakeLists.txt");
                await TestUtilities.CopyResourceToTextFileAsync(scriptPath, "CMakeLists.txt");
            }

            // Step1: Execute gcc
            var (gccExitCode, gccLog) = await TestUtilities.ExecuteAsync(
                outPath, new[] { binPath },
                Path.Combine(binPath, "gcc.exe"),
                $"-I{basePath}",
                incDir,
                libDir,
                cdefs,
                ccflags,
                "-o",
                executablePath,
                sourcePath,
                libs);
            if (gccExitCode != 0)
            {
                throw new Exception("gcc [ExitCode=" + gccExitCode + "]: " + gccLog);
            }

            // Step2: Execute native binary
            var (testExitCode, testLog) = await TestUtilities.ExecuteAsync(
                outPath, new[] { outPath }, executablePath);
            if (testExitCode != 0)
            {
                throw new Exception("test [ExitCode=" + testExitCode + "]: " + testLog);
            }

            return testLog;
        }
    }
}
