// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2018 Ensage.
// </copyright>

using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace Courier_Master
{
    internal static class Program
    {
        private static readonly Menu Menu =
            new Menu("Courier Master", "cb", true, "courier_burst", true).SetFontColor(Color.Aqua);

        private static bool _loaded;
        private static Unit _fountain;

        private static void Main()
        {
            var avoidenemy = new Menu("AvoidEnemy", "AvoidEnemy");
            avoidenemy.AddItem(new MenuItem("AvoidEnemy.AvoidEnemy1", "Enable Avoid Enemy").SetValue(true)
                .SetTooltip("Courier will use Shield"));
            avoidenemy.AddItem(new MenuItem("AvoidEnemy.Range", "Range").SetValue(new Slider(700, 100, 1000)));
            Menu.AddItem(new MenuItem("Forced", "Anti Reuse deliver")
                .SetValue(new KeyBind('0', KeyBindType.Toggle, false))
                .SetTooltip("Courier deliver items to you (antireus indeed)"));
            Menu.AddItem(new MenuItem("Lock", "Lock at fountain").SetValue(new KeyBind('0', KeyBindType.Toggle, false))
                .SetTooltip("Couriers lock at fountain"));
            Menu.AddItem(new MenuItem("Secret Shop", "Secret Shop")
                .SetValue(new KeyBind('0', KeyBindType.Toggle, false))
                .SetTooltip("Courier will go to secret shop"));
            Menu.AddItem(new MenuItem("Cd", "Rate").SetValue(new Slider(150, 30, 300)));
            Menu.AddSubMenu(avoidenemy);
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }    

            
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Utils.SleepCheck("rate"))
                return;

            var me = ObjectManager.LocalHero;
            var couriers = ObjectManager.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == me.Team);


            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || !me.IsAlive)
                    return;
                _loaded = true;
                _fountain = null;
            }

            if (!Game.IsInGame || me == null || couriers == null)
            {
                _loaded = false;
                return;
            }
                
            if (Game.IsPaused) return;

            if (_fountain == null || !_fountain.IsValid)
                _fountain = ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == me.Team && x.ClassId == ClassId.CDOTA_Unit_Fountain);

            //avoid enemy
            foreach (var courier in couriers)
            {
                if (Menu.Item("AvoidEnemy.AvoidEnemy1").GetValue<bool>())
                {
                    var enemies = ObjectManager.GetEntities<Hero>()
                        .Where(x => x.IsAlive && !x.IsIllusion && x.Team != me.Team).ToList();
                    foreach (var enemy in enemies)
                        if (enemy.Distance2D(courier) < Menu.Item("AvoidEnemy.Range").GetValue<Slider>().Value)
                        {
                            var shield = courier.GetAbilityById(AbilityId.courier_shield);
                            if (courier.IsFlying && shield.CanBeCasted())
                                shield.UseAbility();
                        }
                }
                Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
            }
            //Deliver(Anti reuse)
            foreach (var courier in couriers)
            {
                if (Menu.Item("Forced").GetValue<KeyBind>().Active)
                
                    courier.GetAbilityById(AbilityId.courier_take_stash_and_transfer_items).UseAbility();
                
                {
                    Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
                }
            }

            //lock at base
            foreach (var courier in couriers.Where(courier => courier.Distance2D(_fountain) > 900))
            {
                if (Menu.Item("Lock").GetValue<KeyBind>().Active && !Menu.Item("Forced").GetValue<KeyBind>().Active)
                    courier.GetAbilityById(AbilityId.courier_return_to_base).UseAbility();
                {
                    Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
                }
            }
            //secret shop
            foreach (var courier in couriers)
                if (Menu.Item("Secret Shop").GetValue<KeyBind>().Active)
                {
                    courier.GetAbilityById(AbilityId.courier_go_to_secretshop).UseAbility();
                }
            Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Color
            if (Menu.Item("Forced").GetValue<KeyBind>().Active)
                Drawing.DrawText("ANTIREUSE DELIVER", new Vector2((int) HUDInfo.ScreenSizeX() / 2 - 110, 130),
                    new Vector2(26, 26), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
            if (Menu.Item("Lock").GetValue<KeyBind>().Active)
                Drawing.DrawText("LOCK AT BASE", new Vector2((int) HUDInfo.ScreenSizeX() / 2 - 80, 70),
                    new Vector2(26, 26), Color.Blue, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
            if (Menu.Item("Secret Shop").GetValue<KeyBind>().Active)
                Drawing.DrawText("SECRET SHOP", new Vector2((int) HUDInfo.ScreenSizeX() / 2 - 75, 40),
                    new Vector2(26, 26), Color.DarkCyan,
                    FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
        }
    }
}