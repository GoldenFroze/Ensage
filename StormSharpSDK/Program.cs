// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>
//Not ODSHARP
namespace StormSharpSDK
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.SDK.Input;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;
    using Ensage.SDK.TargetSelector;

    [ExportPlugin("StormSharpSDK", HeroId.npc_dota_hero_storm_spirit)]
    public class Program : Plugin
    {
        private readonly IServiceContext context;

        [ImportingConstructor]
        public Program([Import] IServiceContext context)
        {
            this.context = context;
        }

        public StormSharpConfig Config { get; private set; }

        public StormSharp OrbwalkerMode { get; private set; }

        protected override void OnActivate()
        {
            this.Config = new StormSharpConfig();
            var key = KeyInterop.KeyFromVirtualKey((int)this.Config.Key.Value.Key);
            this.Config.Key.Item.ValueChanged += this.HotkeyChanged;

            this.OrbwalkerMode = new StormSharp(key, this.Config, this.context);


            this.context.Orbwalker.RegisterMode(this.OrbwalkerMode);
        }

        protected override void OnDeactivate()
        {
            this.context.Orbwalker.UnregisterMode(this.OrbwalkerMode);
            this.Config.Key.Item.ValueChanged -= this.HotkeyChanged;
            this.Config.Dispose();
        }

        private void HotkeyChanged(object sender, OnValueChangeEventArgs e)
        {
            var keyCode = e.GetNewValue<KeyBind>().Key;
            if (keyCode == e.GetOldValue<KeyBind>().Key)
            {
                return;
            }

            var key = KeyInterop.KeyFromVirtualKey((int)keyCode);
            this.OrbwalkerMode.Key = key;
        }
    }
}