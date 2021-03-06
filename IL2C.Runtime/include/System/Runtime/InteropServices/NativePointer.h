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

#ifndef System_Runtime_InteropServices_NativePointer_H__
#define System_Runtime_InteropServices_NativePointer_H__

#pragma once

#include <il2c.h>

#ifdef __cplusplus
extern "C" {
#endif

/////////////////////////////////////////////////////////////
// System.Runtime.InteropServices.NativePointer

typedef void* System_Runtime_InteropServices_NativePointer;

typedef System_IntPtr_VTABLE_DECL__ System_Runtime_InteropServices_NativePointer_VTABLE_DECL__;

#define System_Runtime_InteropServices_NativePointer_VTABLE__ System_IntPtr_VTABLE__

IL2C_DECLARE_RUNTIME_TYPE(System_Runtime_InteropServices_NativePointer);

#define System_Runtime_InteropServices_NativePointer_op_Implicit__System_IntPtr(value) ((System_Runtime_InteropServices_NativePointer)(value))
#define System_Runtime_InteropServices_NativePointer_op_Implicit__System_Runtime_InteropServices_NativePointer(value) ((intptr_t)(value))

#ifdef __cplusplus
}
#endif

#endif
