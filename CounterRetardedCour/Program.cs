using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

//Ensage Property
//Copyright 2017

namespace AntiCourFeed
{
    internal static class Program
    {
        private static readonly Menu Menu = new Menu("Anti Cour Feeder", "cb", true);
        private static bool _loaded;
        private static Unit _fountain;

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Menu.AddItem(new MenuItem("Lock", "Lock Courier").SetValue(false).SetTooltip("will not allow to leave the base"));
            Menu.AddItem(new MenuItem("AntiReuse", "Anti Reuse").SetValue(false));
            Menu.AddItem(new MenuItem("Cd", "Rate").SetValue(new Slider(50, 5, 200)));
            Menu.AddItem(new MenuItem("MaxRange", "Max Range").SetValue(new Slider(500, 0, 2000)));
            Menu.AddToMainMenu();
        }

        private static void Game_OnUpdate(EventArgs args)
        {

            if (!Utils.SleepCheck("acd.cd"))
                return;

            if (!Menu.Item("Lock").GetValue<bool>())
                return;

            var me = ObjectManager.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                _fountain = null;
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                return;
            }
            if (Game.IsPaused) return;

            var couriers = ObjectManager.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == me.Team);
            if (_fountain == null || !_fountain.IsValid)
            {
                _fountain = ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == me.Team && x.ClassId == ClassId.CDOTA_Unit_Fountain);
            }
            foreach (var courier in couriers.Where(courier => courier.Distance2D(_fountain) > Menu.Item("MaxRange").GetValue<Slider>().Value))
            {
                Debug.Assert(_fountain != null, "_fountain != null");
                var angle = (float)Math.Max(
                    Math.Abs(courier.RotationRad - Utils.DegreeToRadian(courier.FindAngleBetween(_fountain.Position))) - 0.20, 0);
                if (angle == 0) continue;
                courier.Move(_fountain.Position);
                Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "acd.cd");
            }
        }
    }
}
