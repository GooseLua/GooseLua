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
    class GooseProxy
    {
        private readonly Script script;
        private GooseEntity goose => _G.goose;

        [MoonSharpHidden]
        public GooseProxy(Script script)
        {
            this.script = script;
            script.Globals["Speed"] = UserData.CreateStatic<GooseEntity.SpeedTiers>();
            script.Globals["ScreenDirection"] = UserData.CreateStatic<ScreenDirection>();
        }

        [MoonSharpHidden]
        public static void Register()
        {
            UserData.RegisterType<GooseProxy>();
            UserData.RegisterType<VectorProxy>();
            UserData.RegisterType<RigProxy>();
            UserData.RegisterType<ProceduralFeetsProxy>();
            UserData.RegisterType<ScreenDirection>();
            UserData.RegisterType<GooseEntity.SpeedTiers>();
        }

        public DynValue Position
        {
            get => VectorProxy.ToDynValue(script, goose, typeof(GooseEntity).GetField("position"));
            set => goose.position = VectorProxy.ToVector(value);
        }

        public DynValue Velocity
        {
            get => VectorProxy.ToDynValue(script, goose, typeof(GooseEntity).GetField("velocity"));
            set => goose.velocity = VectorProxy.ToVector(value);
        }

        public float Direction
        {
            get => goose.direction;
            set => goose.direction = value;
        }

        public DynValue TargetDirection
        {
            get => VectorProxy.ToDynValue(script, goose, typeof(GooseEntity).GetField("targetDirection"));
            set => goose.targetDirection = VectorProxy.ToVector(value);
        }

        public bool ExtendingNeck
        {
            get => goose.extendingNeck;
            set => goose.extendingNeck = value;
        }

        public DynValue Target
        {
            get => VectorProxy.ToDynValue(script, goose, typeof(GooseEntity).GetField("targetPos"));
            set => goose.targetPos = VectorProxy.ToVector(value);
        }

        public float CurrentSpeed
        {
            get => goose.currentSpeed;
            set => goose.currentSpeed = value;
        }

        public float CurrentAcceleration
        {
            get => goose.currentAcceleration;
            set => goose.currentAcceleration = value;
        }

        public float StepInterval
        {
            get => goose.stepInterval;
            set => goose.stepInterval = value;
        }

        public bool CanDecelerateImmediately
        {
            get => goose.canDecelerateImmediately;
            set => goose.canDecelerateImmediately = value;
        }

        public float Time
        {
            get => SamEngine.Time.time;
        }

        #region Foot Marks
        public float TrackMudEndTime
        {
            get => goose.trackMudEndTime;
            set => goose.trackMudEndTime = value;
        }

        public void AddFootMark(DynValue position)
        {
            goose.footMarks[goose.footMarkIndex].time = SamEngine.Time.time;
            goose.footMarks[goose.footMarkIndex].position = VectorProxy.ToVector(position);
            goose.footMarkIndex++;
            if (goose.footMarkIndex >= goose.footMarks.Length)
            {
                goose.footMarkIndex = 0;
            }
        }

        public void ClearFootMarks()
        {
            for (int i=0; i < goose.footMarks.Length; i++) 
            {
                goose.footMarks[i].time = 0f;
            }
        }
        #endregion

        public DynValue Rig
        {
            get => DynValue.FromObject(script, new RigProxy(script, goose.rig));
            set => UpdateRig(value);
        }

        private void UpdateRig(DynValue value)
        {
            System.Console.WriteLine("Updating rig");
        }

        #region Goose Functions
        public string[] Tasks { get => GetTasks(); }
        
        public string Task
        {
            get => GetTask();
            set => SetTask(value, true);
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

        public void ChooseRandomTask()
        {
            API.Goose.chooseRandomTask(goose);
        }

        public void Roam()
        {
            API.Goose.setTaskRoaming(goose);
        }

        public void Honk()
        {
            API.Goose.playHonckSound();
        }

        public void SetSpeed(GooseEntity.SpeedTiers tier)
        {
            API.Goose.setSpeed(goose, tier);
        }

        public ScreenDirection SetTargetOffscreen(bool canExitTop = false)
        {
            return API.Goose.setTargetOffscreen(goose, canExitTop);
        }

        public bool IsGooseAtTarget(float distance)
        {
            return API.Goose.isGooseAtTarget(goose, distance);
        }

        public float DistanceToTarget
        {
            get => API.Goose.getDistanceToTarget(goose);
        }
        #endregion

        #region Deprecated
        // Deprecated functions for compatibility
        public string[] GetTasks() => API.TaskDatabase.getAllLoadedTaskIDs();
        public void SetPosition(float x, float y) => goose.position = new Vector2(x, y);
        public void SetTarget(float x, float y) => goose.targetPos = new Vector2(x, y);
        public string GetTask()
        {
            if (goose.currentTask == -1)
            {
                return null;
            }
            return API.TaskDatabase.getAllLoadedTaskIDs()[goose.currentTask];
        }
        #endregion
    }

    class VectorProxy
    {
        private readonly object obj;
        private readonly FieldInfo field;

        [MoonSharpHidden]
        private VectorProxy(object obj, FieldInfo field)
        {
            this.obj = obj;
            this.field = field;
        }

        [MoonSharpHidden]
        internal static DynValue ToDynValue(Script script, object obj, FieldInfo field)
        {
            return DynValue.FromObject(script, new VectorProxy(obj, field));
        }

        [MoonSharpHidden]
        internal static Vector2 ToVector(DynValue value)
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

    class RigProxy
    {
        private Rig rig;
        private Script script;

        [MoonSharpHidden]
        internal RigProxy(Script script, Rig rig)
        {
            this.script = script;
            this.rig = rig;
            this.feets = DynValue.FromObject(script, new ProceduralFeetsProxy(script, rig.feets));
        }

        /* Feets */
        public DynValue feets { get; private set; }

        /* Under Body*/
        public const int UnderBodyRadius = Rig.UnderBodyRadius;
        public const int UnderBodyLength = Rig.UnderBodyLength;
        public const int UnderBodyElevation = Rig.UnderBodyElevation;
        public DynValue underbodyCenter
        {
            get => VectorProxy.ToDynValue(script, rig, typeof(Rig).GetField("underbodyCenter"));
            set => rig.underbodyCenter = VectorProxy.ToVector(value);
        }

        /* Body */
        public const int BodyRadius = Rig.BodyRadius;
        public const int BodyLength = Rig.BodyLength;
        public const int BodyElevation = Rig.BodyElevation;
        public DynValue bodyCenter
        {
            get => VectorProxy.ToDynValue(script, rig, typeof(Rig).GetField("bodyCenter"));
            set => rig.bodyCenter = VectorProxy.ToVector(value);
        }

        /* Necc */
        // Properties
        public const int NeccRadius = Rig.NeccRadius;
        public const int NeccHeight1 = Rig.NeccHeight1;
        public const int NeccExtendForward1 = Rig.NeccExtendForward1;
        public const int NeccHeight2 = Rig.NeccHeight2;
        public const int NeccExtendForward2 = Rig.NeccExtendForward2;
        public float neckLerpPercent
        {
            get => rig.neckLerpPercent;
            set => rig.neckLerpPercent = value;
        }
        public DynValue neckCenter
        {
            get => VectorProxy.ToDynValue(script, rig, typeof(Rig).GetField("neckCenter"));
            set => rig.neckCenter = VectorProxy.ToVector(value);
        }
        public DynValue neckBase
        {
            get => VectorProxy.ToDynValue(script, rig, typeof(Rig).GetField("neckBase"));
            set => rig.neckBase = VectorProxy.ToVector(value);
        }
        public DynValue neckHeadPoint
        {
            get => VectorProxy.ToDynValue(script, rig, typeof(Rig).GetField("neckHeadPoint"));
            set => rig.neckHeadPoint = VectorProxy.ToVector(value);
        }

        /* Head */
        public const int HeadRadius1 = Rig.HeadRadius1;
        public const int HeadLength1 = Rig.HeadLength1;
        public const int HeadRadius2 = Rig.HeadRadius2;
        public const int HeadLength2 = Rig.HeadLength2;
        public DynValue head1EndPoint
        {
            get => VectorProxy.ToDynValue(script, rig, typeof(Rig).GetField("head1EndPoint"));
            set => rig.head1EndPoint = VectorProxy.ToVector(value);
        }
        public DynValue head2EndPoint
        {
            get => VectorProxy.ToDynValue(script, rig, typeof(Rig).GetField("head2EndPoint"));
            set => rig.head2EndPoint = VectorProxy.ToVector(value);
        }

        /* Eyes */
        public const int EyeRadius = Rig.EyeRadius;
        public const int EyeElevation = Rig.EyeElevation;
        public const float IPD = Rig.IPD;
        public const float EyesForward = Rig.EyesForward;
    }

    public class ProceduralFeetsProxy
    {
        private Script script;
        private ProceduralFeets feets;

        [MoonSharpHidden]
        public ProceduralFeetsProxy(Script script, ProceduralFeets feets)
        {
            this.script = script;
            this.feets = feets;
        }

        public DynValue lFootPos
        {
            get => VectorProxy.ToDynValue(script, feets, typeof(ProceduralFeets).GetField("lFootPos"));
            set => feets.lFootPos = VectorProxy.ToVector(value);
        }

        public DynValue rFootPos
        {
            get => VectorProxy.ToDynValue(script, feets, typeof(ProceduralFeets).GetField("rFootPos"));
            set => feets.rFootPos = VectorProxy.ToVector(value);
        }

        public float lFootMoveTimeStart
        {
            get => feets.lFootMoveTimeStart;
            set => feets.lFootMoveTimeStart = value;
        }

        public float rFootMoveTimeStart
        {
            get => feets.rFootMoveTimeStart;
            set => feets.rFootMoveTimeStart = value;
        }

        public DynValue lFootMoveOrigin
        {
            get => VectorProxy.ToDynValue(script, feets, typeof(ProceduralFeets).GetField("lFootMoveOrigin"));
            set => feets.lFootMoveOrigin = VectorProxy.ToVector(value);
        }

        public DynValue rFootMoveOrigin
        {
            get => VectorProxy.ToDynValue(script, feets, typeof(ProceduralFeets).GetField("rFootMoveOrigin"));
            set => feets.rFootMoveOrigin = VectorProxy.ToVector(value);
        }

        public DynValue lFootMoveDir
        {
            get => VectorProxy.ToDynValue(script, feets, typeof(ProceduralFeets).GetField("lFootMoveDir"));
            set => feets.lFootMoveDir = VectorProxy.ToVector(value);
        }

        public DynValue rFootMoveDir
        {
            get => VectorProxy.ToDynValue(script, feets, typeof(ProceduralFeets).GetField("rFootMoveDir"));
            set => feets.rFootMoveDir = VectorProxy.ToVector(value);
        }

        public int feetDistanceApart
        {
            get => feets.feetDistanceApart;
            set => feets.feetDistanceApart = value;
        }

        public const float wantStepAtDistance = ProceduralFeets.wantStepAtDistance;
        public const float overshootFraction = ProceduralFeets.overshootFraction;
    }
}
