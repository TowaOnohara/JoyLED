using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace JoyLED
{
    public partial class Program
    {
        Queue oldPos = new Queue();
        int MaxQueue = 4;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            // 初期化
            //------------------------------------------
            joystick.Calibrate();                   // ゼロ調整
            TurnOffALL(led7r);                      // 消灯

            // イベント登録
            //------------------------------------------
            joystick.JoystickPressed += joystick_JoystickPressed;
            joystick.JoystickReleased += joystick_JoystickReleased;

            // タイマー処理開始
            //------------------------------------------
            GT.Timer polling = new GT.Timer(200);
            polling.Tick += polling_Tick;
            polling.Start();
        }

        // Releasedイベント：中心LEDを消灯します。
        //------------------------------------------
        void joystick_JoystickReleased(Joystick sender, Joystick.JoystickState state)
        {
            led7r.TurnLightOff(7);
        }

        // Pressedイベント：中心LEDを点灯します。
        //------------------------------------------
        void joystick_JoystickPressed(Joystick sender, Joystick.JoystickState state)
        {
            led7r.TurnLightOn(7, true);
        }


        // 指定の値が、目的値の±0.2範囲に入っているか判定します。
        //------------------------------------------
        bool Judgement(double value, double right) 
        {
            if (((right + 0.2) > value) && ((right - 0.2) < value)) { return true;  }
            else                                                    { return false; }
        }

        // 「上」「下」「上」「右下」コマンドが入力されたか確認します。
        //------------------------------------------
        bool CheckCommand(Queue olds, int NewVale) 
        {
            if (olds.Count < MaxQueue) 
            {
                olds.Enqueue(NewVale);
                return false;
            }
            else 
            {
                olds.Dequeue();
                olds.Enqueue(NewVale);
                object[] ary = olds.ToArray();
                if (((int)ary[0] == 1) &&   // 「上」
                    ((int)ary[1] == 4) &&   // 「下」
                    ((int)ary[2] == 1) &&   // 「上」
                    ((int)ary[3] == 3))     // 「右下」
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }

        // JoyStick位置に対応した番号を取得します。
        //------------------------------------------
        int GetPositionNumber(Joystick.Position pos) 
        {
            if (Judgement(pos.X,  0) && Judgement(pos.Y,  1)) { return 1; }
            if (Judgement(pos.X,  1) && Judgement(pos.Y,  1)) { return 2; }
            if (Judgement(pos.X,  1) && Judgement(pos.Y, -1)) { return 3; }
            if (Judgement(pos.X,  0) && Judgement(pos.Y, -1)) { return 4; }
            if (Judgement(pos.X, -1) && Judgement(pos.Y, -1)) { return 5; }
            if (Judgement(pos.X, -1) && Judgement(pos.Y,  1)) { return 6; }
            return 0;
        }

        // LED7R の周囲のみを消灯します。
        //------------------------------------------
        void TurnOffAround(LED7R led) 
        {
            for(int i = 1; i<=6; i++){
                led.TurnLightOff(i);
            }
        }

        // LED7R を全て消灯します。
        //------------------------------------------
        void TurnOffALL(LED7R led)
        {
            for (int i = 1; i <= 7; i++)
            {
                led.TurnLightOff(i);
            }
        }

        // タイマーイベント
        //------------------------------------------
        void polling_Tick(GT.Timer timer)
        {
            // JoyStick位置に応じてLEDを点灯させます。
            Joystick.Position pos = joystick.GetPosition();     // 位置情報取得
            int posNum = GetPositionNumber(pos);                // 対応した番号を取得
            if (posNum == 0) { TurnOffAround(led7r); }          // 0 の場合は消灯します。
            else { led7r.TurnLightOn(posNum, true); }           // 対応位置を点灯させます。

            // コマンドが入力されているか確認します。
            if ((posNum != 0) && (true == CheckCommand(oldPos, posNum))) 
            {
                TurnOffALL(led7r);                              // 一度消灯させます
                led7r.Animate(300, true, true, true);           // 300msec周期でアニメーション点灯させます。
                TurnOffALL(led7r);                              // 一度消灯させます。
                led7r.Animate(300, true, true, true);           // 300msec周期でアニメーション点灯させます。
                TurnOffALL(led7r);                              // 一度消灯させます。
            }
        }
    }
}
