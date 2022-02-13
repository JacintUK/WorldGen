using System;
using OpenTK;
using OpenTK.Mathematics;

namespace WorldGen
{
    public class TKEvent : EventArgs
    {
        public enum Type
        {
            Window = 0,
            Keyboard,
            TextEditing,
            TextInput,
            MouseMotion,
            MouseButton,
            MouseWheel,
            JoyAxis,
            JoyBall,
            JoyHat,
            JoyButton,
            JoyDevice,
            ControllerAxis,
            ControllerButton,
            ControllerDevice,
            AudioDeviceEvent,
            Quit,
            User,
            SystemWindow,
            TouchFinger,
            MultiGesture,
            Finger,
            Drop
        }

        public Type EventType { get; protected set; }

    }

    public class KeyBoardEvent : TKEvent
    {
        public  bool IsKeyDown { get; private set; }
        public OpenTK.Windowing.GraphicsLibraryFramework.Keys Key { get; private set; }
        public KeyBoardEvent(bool IsKeyDown, OpenTK.Windowing.GraphicsLibraryFramework.Keys Key)
        {
            EventType = Type.Keyboard;
            this.IsKeyDown = IsKeyDown;
            this.Key = Key;
        }
    }

    public class MouseButtonEvent : TKEvent
    {
        public bool IsButtonDown { get; private set; }
        public OpenTK.Windowing.GraphicsLibraryFramework.MouseButton Button { get; private set; }
        public MouseButtonEvent(bool IsButtonDown, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton Button)
        {
            EventType = Type.MouseButton;
            this.IsButtonDown = IsButtonDown;
            this.Button = Button;
        }
    }

    public class MouseWheelEvent : TKEvent
    {
        public Vector2 Value { get; private set; }
        public MouseWheelEvent(Vector2 WheelValue)
        {
            EventType = Type.MouseWheel;
            Value = WheelValue;
        }
    }

    public class MouseMotionEvent : TKEvent
    {
        public Vector2i Position { get; private set; }
        public MouseMotionEvent(Vector2i newPosition)
        {
            EventType = Type.MouseMotion;
            Position = newPosition;
        }
    }

    public class TextInputEvent : TKEvent
    {
        public string Text { get; private set; }
        public TextInputEvent(string Text)
        {
            EventType = Type.TextInput;
            this.Text = Text;
        }
    }
}
