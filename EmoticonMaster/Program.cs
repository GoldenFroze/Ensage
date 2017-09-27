using System;
using System.Collections.Generic;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX;

namespace EmoticonsMaster
{
    public class Program
    {
        private const int WmKeyup = 0x0101;

        private static readonly Menu Menu =
            new Menu("Emoticons Master", "cb", true, "rune_bounty", true).SetFontColor(Color.Blue);

        public static Dictionary<string, DotaTexture> _textureCache = new Dictionary<string, DotaTexture>();
        private static Vector2 startloc, diff;
        private static bool _leftMouseIsPress, leftMouseIsHold, team = true, hold, active, min = true;

        private static readonly string[] name =
        {
            "blush", "cheeky", "cool", "crazy", "cry", "disapprove", "doubledamage", "facepalm", "happytears", "haste",
            "hex", "highfive", "huh", "hush", "illusion", "invisibility", "laugh", "rage", "regeneration", "sad",
            "sick", "sleeping", "smile", "surprise", "wink", "aaaah", "burn", "hide", "iceburn", "tears", "fail",
            "goodjob", "headshot", "heart", "horse", "techies", "grave", "puppy", "cocky", "devil", "happy", "thinking",
            "tp", "salty", "angel", "blink", "bts3_bristle", "stunned", "disappear", "fire", "bounty", "troll", "gross",
            "ggdire", "ggradiant", "yolo", "throwgame", "aegis2015", "eyeroll", "dac15_tired", "dac15_blush",
            "dac15_face", "dac15_cool", "dac15_duel", "dac15_transform", "dac15_stab", "dac15_frog", "dac15_surprise",
            "bts3_bristle", "bts3_godz", "bts3_lina", "bts3_merlini", "bts3_rosh", "ti4copper", "ti4bronze",
            "ti4silver", "ti4gold", "ti4platinum", "ti4diamond", "bc_emoticon_hundred", "bc_emoticon_fire",
            "bc_emoticon_okay", "bc_emoticon_check", "bc_emoticon_eyes", "bc_emoticon_frog", "bc_emoticon_flex",
            "monkey_king_ti6_charm", "huh_ti5_charm", "aegis"
        };

        private static readonly string[] emo =
        {
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", ""
        };

        private static void Main(string[] args)
        {
            var Position = new Menu("Position", "position");
            Position.AddItem(
                new MenuItem("xposition", "X Position").SetValue(new Slider(791, 1, (int)HUDInfo.ScreenSizeX())));
            Position.AddItem(
                new MenuItem("yposition", "Y Position").SetValue(new Slider(226, 1, (int)HUDInfo.ScreenSizeY())));
            Menu.AddSubMenu(Position);
            Menu.AddToMainMenu();

            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintMessage("<font color='#00ff00'>Emoticons has been Loaded by GoldenFroze </font>");
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            _leftMouseIsPress = args.WParam == 1;
            if (args.Msg != WmKeyup && args.WParam == 1)
                leftMouseIsHold = true;
            else leftMouseIsHold = false;

            active = Game.IsChatOpen;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || !active) return;
            startloc = new Vector2(Menu.Item("xposition").GetValue<Slider>().Value,
                Menu.Item("yposition").GetValue<Slider>().Value);

            if (min)
            {
                Drawing.DrawRect(startloc, new Vector2(300, 300), GetTexture("materials/ensage_ui/menu/itembg1.vmat"));
                DrawToggleButton(startloc + new Vector2(0, 280), 300, 20, ref team);
                for (var i = 0; i < 89; i++)
                    DrawEmoButton(startloc + new Vector2(15 + i % 11 * 25, 25 + i / 11 * 25), 20, 20, emo[i], name[i]);
            }
            else
            {
                Drawing.DrawRect(startloc, new Vector2(300, 20), GetTexture("materials/ensage_ui/menu/itembg1.vmat"));
            }
            DragButton(startloc, 280, 20);
            DrawMinButton(startloc + new Vector2(280, 0), 20, 20, ref min);
        }

        public static DotaTexture GetTexture(string name)
        {
            if (_textureCache.ContainsKey(name)) return _textureCache[name];

            return _textureCache[name] = Drawing.GetTexture(name);
        }

        private static void DrawEmoButton(Vector2 loc, float w, float h, string emoticon, string emo_name)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, loc.X, loc.Y, w, h);
            if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn && !hold)
            {
                Game.ExecuteCommand((team ? "say_team " : "say ") + emoticon);
                Utils.Sleep(250, "ClickButtonCd");
            }
            if (emo_name == "bounty")
                Drawing.DrawRect(loc, new Vector2(w, h),
                    GetTexture("materials/ensage_ui/minirunes/" + emo_name + ".vmat"));
            else
                Drawing.DrawRect(loc, new Vector2(w, h),
                    GetTexture("materials/ensage_ui/emoticons/" + emo_name + ".vmat"));
            var newColor = isIn
                ? (_leftMouseIsPress ? new Color(0, 0, 0, 150) : new Color(0, 0, 0, 70))
                : new Color(0, 0, 0, 0);
            Drawing.DrawRect(loc, new Vector2(w, h), newColor);
        }

        private static void DragButton(Vector2 loc, float w, float h)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, loc.X, loc.Y, w, h);

            if (leftMouseIsHold && Utils.SleepCheck("HoldButtonCd") && isIn && !hold)
            {
                hold = true;
                diff = Game.MouseScreenPosition - startloc;
                Utils.Sleep(250, "HoldButtonCd");
            }
            else if (!leftMouseIsHold)
            {
                hold = false;
            }
            if (hold)
            {
                startloc = Game.MouseScreenPosition - diff;
                Menu.Item("xposition").SetValue(new Slider((int)startloc.X, 1, (int)HUDInfo.ScreenSizeX()));
                Menu.Item("yposition").SetValue(new Slider((int)startloc.Y, 1, (int)HUDInfo.ScreenSizeY()));
            }
            Drawing.DrawRect(loc, new Vector2(w, h), new Color(0, 0, 0, 150));
            Drawing.DrawText("          Emoticons", loc + new Vector2(103, 2), Color.Blue,
                FontFlags.AntiAlias | FontFlags.DropShadow);
        }

        private static void DrawToggleButton(Vector2 loc, float w, float h, ref bool clicked)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, loc.X, loc.Y, w, h);
            if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
            {
                clicked = !clicked;
                Utils.Sleep(250, "ClickButtonCd");
            }
            var newColor = isIn
                ? (_leftMouseIsPress ? new Color(0, 0, 0, 200) : new Color(0, 0, 0, 100))
                : new Color(0, 0, 0, 150);
            Drawing.DrawRect(loc, new Vector2(w, h), newColor);
            Drawing.DrawText(clicked ? "To (Allies)" : "To All", loc + new Vector2(130, 2), Color.Blue,
                FontFlags.AntiAlias | FontFlags.DropShadow);
        }

        private static void DrawMinButton(Vector2 loc, float w, float h, ref bool clicked)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, loc.X, loc.Y, w, h);
            if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
            {
                clicked = !clicked;
                Utils.Sleep(250, "ClickButtonCd");
            }
            var newColor = isIn
                ? (_leftMouseIsPress ? new Color(0, 0, 0, 200) : new Color(0, 0, 0, 100))
                : new Color(0, 0, 0, 150);
            Drawing.DrawRect(loc, new Vector2(w, h), newColor);
            Drawing.DrawText("_", loc + new Vector2(6, 2), Color.Blue,
                FontFlags.AntiAlias | FontFlags.DropShadow);
        }
    }
}