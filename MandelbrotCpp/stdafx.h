// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A
// copy of the license can be found in the License.html file at the root of this distribution.
// If you cannot locate the  Microsoft Public License, please send an email to
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <windowsx.h>

#include <cassert>
#include <cstdio>
#include <cwchar>
#include <chrono>
#include <memory>
#include <vector>

#include <amp.h>
#include <amp_graphics.h>
#include <amp_math.h>
#include <ppl.h>
#include <ppltasks.h>

#include <d3d11_2.h>
#include <directxmath.h>
#include <directxcolors.h>
