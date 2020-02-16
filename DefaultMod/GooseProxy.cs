// 
// GooseProxy.cs
// Copyright (C) 2020 Jesús A. Álvarez
// This file is dual-licensed under the GNU GPL 3.0 and MIT Licenses
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// GNU GENERAL PUBLIC LICENSE
// Version 3, 29 June 2007
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 
using System.Linq;
using System.Reflection;
using GooseShared;
using MoonSharp.Interpreter;
using SamEngine;

namespace GooseLua
{
    class VectorProxy
    {
        private readonly object obj;
        private readonly FieldInfo field;

        [MoonSharpHidden]
        public VectorProxy(object obj, FieldInfo field)
        {
            this.obj = obj;
            this.field = field;
        }

        private Vector2 Vector
        {
            get => (Vector2)field.GetValue(obj);
        }

        public float X
        {
            get => Vector.x;
            set => field.SetValue(obj, new Vector2(value, Vector.y));
        }

        public float Y
        {
            get => Vector.y;
            set => field.SetValue(obj, new Vector2(Vector.x, value));
        }
    }

    class GooseProxy
    {
        private Script script => _G.LuaState;
        private GooseEntity goose => _G.goose;

        [MoonSharpHidden]
        private Vector2 ToVector(DynValue value)
        {
            double x, y;
            switch (value.Type)
            {
                case DataType.Table when value.Table.Get("x").IsNotNil() && value.Table.Get("y").IsNotNil():
                    x = value.Table.Get("x").Number;
                    y = value.Table.Get("y").Number;
                    break;
                case DataType.Table when value.Table.Get(1).IsNotNil() && value.Table.Get(2).IsNotNil() && value.Table.Length == 2:
                    x = value.Table.Get(1).Number;
                    y = value.Table.Get(2).Number;
                    break;
                case DataType.Tuple when value.Tuple.Length == 2:
                    x = value.Tuple[0].Number;
                    y = value.Tuple[1].Number;
                    break;
                default:
                    throw new ScriptRuntimeException("Cannot convert value to vector: " + value.ToDebugPrintString());
            }
            return new Vector2((float)x, (float)y);
        }

        public DynValue Position
        {
            get => DynValue.FromObject(script, new VectorProxy(goose, typeof(GooseEntity).GetField("position")));
            set => goose.position = ToVector(value);
        }
        public void SetPosition(float x, float y) => goose.position = new Vector2(x, y);

        public DynValue Target
        {
            get => DynValue.FromObject(script, new VectorProxy(goose, typeof(GooseEntity).GetField("targetPos")));
            set => goose.targetPos = ToVector(value);
        }
        public void SetTarget(float x, float y) => goose.targetPos = new Vector2(x, y);

        public string[] Tasks { get => GetTasks(); }
        public string[] GetTasks() => API.TaskDatabase.getAllLoadedTaskIDs();

        public string Task
        {
            get => GetTask();
            set => SetTask(value, true);
        }
        public string GetTask()
        {
            if (goose.currentTask == -1)
            {
                return null;
            }
            return API.TaskDatabase.getAllLoadedTaskIDs()[goose.currentTask];
        }

        public void SetTask(string taskName, bool honk = true)
        {
            string[] tasks = API.TaskDatabase.getAllLoadedTaskIDs();
            if (tasks.Any(taskName.Equals))
            {
                API.Goose.setCurrentTaskByID(goose, taskName, honk);
            } else
            {
                throw new ScriptRuntimeException("Unknown task \"" + taskName + "\".");
            }
        }
    }
}
