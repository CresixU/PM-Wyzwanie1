using OpenTK;
using OpenTK.Graphics.OpenGL4;
using GLFW;
using GlmSharp;

using Shaders;
using Models;

namespace PMLabs
{
    //Implementacja interfejsu dostosowującego metodę biblioteki Glfw służącą do pozyskiwania adresów funkcji i procedur OpenGL do współpracy z OpenTK.
    public class BC: IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Glfw.GetProcAddress(procName);
        }
    }

    class Program
    {   
        static Cube cubeLeg1 = new();
        static Cube cubeLeg2 = new();
        static Cube cubeLeg3 = new();
        static Cube cubeLeg4 = new();
        static Cube cubeCounterTop = new();
        static Teapot teapot = new();
        static Sphere sphereBubble = new Sphere(0.1f, 20f, 20f);
        static Sphere sphereBubble2 = new Sphere(0.1f, 20f, 20f);

        private static float _speedY = 0f;
        private static float _speedX = 0f;
        private static float _moveY = 0f;
        private static float _moveX = 0f;
        private static float _moveZ = 0f;
        private static readonly float firstStep = 3f;
        private static float calculatedTime = 0f;
        private static float calculatedBubbleScale = 0f;
        private static float bubbleMaxHeight = 2f;
        private static float bubbleHeightInFlyingMode = 0f;

        public static void InitOpenGLProgram(Window window)
        {
            // Czyszczenie okna na kolor czarny
            GL.ClearColor(0, 0, 0, 1);

            // Ładowanie programów cieniujących
            DemoShaders.InitShaders("Shaders\\");

            Glfw.SetKeyCallback(window, KeyProcessor);
        }

        public static void DrawScene(Window window, float time, float angleX, float angleY, float moveX, float moveY, float moveZ)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit);

            mat4 V = mat4.LookAt(
                new vec3(1f*moveX, 1f+moveZ, 1f*moveY-5f),
                new vec3(1f*moveX, 1f*moveZ, 1f*moveY),
                new vec3(0f, 1f, 0f));
            mat4 P = mat4.Perspective(glm.Radians(50.0f), 1.0f, 1.0f, 50.0f);

            DemoShaders.spConstant.Use();
            GL.UniformMatrix4(DemoShaders.spConstant.U("P"), 1, false, P.Values1D);
            GL.UniformMatrix4(DemoShaders.spConstant.U("V"), 1, false, V.Values1D);


            mat4 McubeLeg1 = mat4.Identity;
            McubeLeg1 *= mat4.Translate(new vec3(1f, 0f, 1f));
            McubeLeg1 *= mat4.Scale(new vec3(0.1f, 0.5f, 0.1f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"),1,false, McubeLeg1.Values1D);
            cubeLeg1.drawWire();

            mat4 McubeLeg2 = mat4.Identity;
            McubeLeg2 *= mat4.Translate(new vec3(-1f, 0f, 1f));
            McubeLeg2 *= mat4.Scale(new vec3(0.1f, 0.5f, 0.1f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, McubeLeg2.Values1D);
            cubeLeg2.drawWire();

            mat4 McubeLeg3 = mat4.Identity;
            McubeLeg3 *= mat4.Translate(new vec3(1f, 0f, -1f));
            McubeLeg3 *= mat4.Scale(new vec3(0.1f, 0.5f, 0.1f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, McubeLeg3.Values1D);
            cubeLeg3.drawWire();

            mat4 McubeLeg4 = mat4.Identity;
            McubeLeg4 *= mat4.Translate(new vec3(-1f, 0f, -1f));
            McubeLeg4 *= mat4.Scale(new vec3(0.1f, 0.5f, 0.1f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, McubeLeg4.Values1D);
            cubeLeg4.drawWire();

            mat4 McubeCounterTop = mat4.Identity;
            McubeCounterTop *= mat4.Translate(new vec3(0f, 0.4f, 0f));
            McubeCounterTop *= mat4.Scale(new vec3(1.1f, 0.1f, 1.1f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, McubeCounterTop.Values1D);
            cubeCounterTop.drawWire();

            mat4 Mteapot = mat4.Identity;
            Mteapot *= mat4.Translate(new vec3(0f, 0.7f, 0.5f));
            Mteapot *= mat4.Scale(new vec3(0.5f, 0.5f, 0.5f));
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, Mteapot.Values1D);
            teapot.drawWire();

            mat4 MsphereBubble = mat4.Identity;
            AnimateBubble(ref MsphereBubble, time);
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, MsphereBubble.Values1D);
            sphereBubble.drawWire();

            mat4 MsphereBubble2 = mat4.Identity;
            AnimateBubble(ref MsphereBubble2, time-13);
            GL.UniformMatrix4(DemoShaders.spConstant.U("M"), 1, false, MsphereBubble2.Values1D);
            sphereBubble2.drawWire();

            Glfw.SwapBuffers(window);
        }

        public static void FreeOpenGLProgram(Window window)
        {
            // Możesz dodać odpowiednie czyszczenie zasobów tutaj, jeśli jest to konieczne
        }

        static void Main(string[] args)
        {
            var angleX = 0f;
            var angleY = 0f;
            var moveX = 0f;
            var moveY = 0f;
            var moveZ = 0f;

            Glfw.Init();

            Window window = Glfw.CreateWindow(500, 500, "Programowanie multimedialne", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SwapInterval(1);

            GL.LoadBindings(new BC()); //Pozyskaj adresy implementacji poszczególnych procedur OpenGL

            InitOpenGLProgram(window);

            Glfw.Time = 0;
            var oldTime = 0f;
            while (!Glfw.WindowShouldClose(window))
            {
                var newTime = oldTime - (float)Glfw.Time;
                angleX += _speedX * newTime;
                angleY += _speedY * newTime;
                moveX += -_moveX * newTime;
                moveY += -_moveY * newTime;
                moveZ += -_moveZ * newTime;
                oldTime = (float)Glfw.Time;

                DrawScene(window, (float)Glfw.Time, angleX, angleY, moveX, moveY, moveZ);
                Glfw.PollEvents();
            }


            FreeOpenGLProgram(window);

            Glfw.Terminate();
        }

        private static void AnimateBubble(ref mat4 M_bubble, float time)
        {
            calculatedTime = time % 10;
            calculatedBubbleScale = 0f;
            bubbleMaxHeight = 2f;
            bubbleHeightInFlyingMode = 0.87f + calculatedTime - firstStep;

            if (calculatedTime < 0.2)
            {
                M_bubble *= mat4.Translate(new vec3(0.5f, 0.87f, 0.5f));
                M_bubble *= mat4.Scale(new vec3(calculatedBubbleScale, calculatedBubbleScale, calculatedBubbleScale));
            }
            else if(calculatedTime < firstStep)
            {
                calculatedBubbleScale = calculatedTime / 10f;
                M_bubble *= mat4.Translate(new vec3(0.5f, 0.87f, 0.5f));
                M_bubble *= mat4.Scale(new vec3(calculatedBubbleScale, calculatedBubbleScale, calculatedBubbleScale));
            }
            else if(calculatedTime <= 5f && bubbleHeightInFlyingMode <= bubbleMaxHeight)
            {
                calculatedBubbleScale = calculatedTime / 10f;
                M_bubble *= mat4.Translate(new vec3(0.5f, bubbleHeightInFlyingMode, 0.5f));
                M_bubble *= mat4.Scale(new vec3(calculatedBubbleScale, calculatedBubbleScale, calculatedBubbleScale));
            }
            else
                M_bubble *= mat4.Scale(new vec3(0, 0, 0));
        }

        private static void KeyProcessor(IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            const float moveAmout = 2f;
            if (state == InputState.Press)
            {
                if (key == Keys.Left) _speedY = -3.14f;
                if (key == Keys.Right) _speedY = 3.14f;
                if (key == Keys.Up) _speedX = -3.14f;
                if (key == Keys.Down) _speedX = 3.14f;
                if (key == Keys.W) _moveY = moveAmout;
                if (key == Keys.S) _moveY = -moveAmout;
                if (key == Keys.A) _moveX = moveAmout;
                if (key == Keys.D) _moveX = -moveAmout;
                if (key == Keys.Z) _moveZ = moveAmout;
                if (key == Keys.X) _moveZ = -moveAmout;

            }
            if (state == InputState.Release)
            {
                if (key == Keys.Left) _speedY = 0;
                if (key == Keys.Right) _speedY = 0;
                if (key == Keys.Up) _speedX = 0;
                if (key == Keys.Down) _speedX = 0;
                if (key == Keys.W) _moveY = 0;
                if (key == Keys.S) _moveY = 0;
                if (key == Keys.A) _moveX = 0;
                if (key == Keys.D) _moveX = 0;
                if (key == Keys.Z) _moveZ = 0;
                if (key == Keys.X) _moveZ = 0;
            }
        }
    }
}