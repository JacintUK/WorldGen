﻿/*
 * Copyright 2018 David Ian Steele
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using OpenTK.Mathematics;

namespace WorldGen
{
    interface IMesh
    {
        int Length { get; }

        void SetColor(int index, ref Vector4 color);
        Vector4 GetColor(int index);
        void SetPosition(int index, ref Vector3 position);
        Vector3 GetPosition(int index);

        float GetPrimary(int index);
    }
}
