// <copyright file="StormSharp.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>
//NOT ODSHARP
namespace StormSharpSDK
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common.Enums;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Ensage.Common.Threading;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Handlers;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Input;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Prediction.Collision;
    using Ensage.SDK.TargetSelector;
    using Ensage.SDK.Abilities.Items;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;

    using log4net;

    using PlaySharp.Toolkit.Logging;
    using PlaySharp.Toolkit.Helper.Annotations;

    using SharpDX;

    using AbilityId = Ensage.AbilityId;
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;
    using System.Collections.Generic;

    [PublicAPI]
    public class StormSharp : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public StormSharp(
            Key key,
            StormSharpConfig config,
            IServiceContext context)
            : base(context, key)
        {

            this.context = context;
            this.Config = config;
            this.TargetSelector = context.TargetSelector;
            this.Inventory = context.Inventory;
            this.Prediction = context.Prediction;

        }

        public StormSharpConfig Config { get; }
        private IPrediction Prediction { get; }
        private ITargetSelectorManager TargetSelector { get; }
        private readonly IServiceContext context;

        [ItemBinding]
        public item_hurricane_pike HurricanePike { get; private set; }

        [ItemBinding]
        public item_shivas_guard ShivasGuard { get; private set; }

        private Ability Vortex { get; set; }

        private Ability Lightning { get; set; }

        private Ability Remnant { get; set; }


        private TaskHandler KillStealHandler { get; set; }
        private IInventoryManager Inventory { get; }

        [ItemBinding]
        public item_blink BlinkDagger { get; private set; }

        [ItemBinding]
        public item_bloodthorn BloodThorn { get; private set; }

        [ItemBinding]
        public item_orchid Orchid { get; private set; }
        [ItemBinding]
        public item_mjollnir Mjollnir { get; private set; }

        [ItemBinding]
        public item_rod_of_atos RodofAtos { get; private set; }

        [ItemBinding]
        public item_sheepstick SheepStick { get; private set; }

        [ItemBinding]
        public item_veil_of_discord VeilofDiscord { get; private set; }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            this.KillStealHandler.RunAsync();

            var target = this.TargetSelector.Active.GetTargets()
                .FirstOrDefault(x => !x.IsInvulnerable() && !UnitExtensions.IsMagicImmune(x) && x.IsAlive);

            var silenced = UnitExtensions.IsSilenced(this.Owner);

            var sliderValue = this.Config.UseBlinkPrediction.Item.GetValue<Slider>().Value;



            if ((this.BlinkDagger != null) &&
                (this.BlinkDagger.Item.IsValid) &&
                target != null && Owner.Distance2D(target) <= 1200 + sliderValue &&
                !(Owner.Distance2D(target) <= 400) &&
                this.BlinkDagger.Item.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled(this.BlinkDagger.Item.Name))
            {
                var l = (this.Owner.Distance2D(target) - sliderValue) / sliderValue;
                var posA = this.Owner.Position;
                var posB = target.Position;
                var x = (posA.X + (l * posB.X)) / (1 + l);
                var y = (posA.Y + (l * posB.Y)) / (1 + l);
                var position = new Vector3((int) x, (int) y, posA.Z);

                Log.Debug("Using BlinkDagger");
                this.BlinkDagger.UseAbility(position);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }


            //Check if we're silenced, our target is alive, and we have a target.
            var UltDistance = Config.DistanceForUlt.Item.GetValue<Slider>().Value;

            //Check for distance to target and push against slider value
            if (target != null && target.IsAlive
                && Owner.Distance2D(target) >= 400 && Owner.Distance2D(target) <= UltDistance
                && Config.AbilityToggler.Value.IsEnabled(Lightning.Name) && !silenced)
            {
                //Based on whether they are moving or not, predict where they will be.
                if (target.IsMoving)
                {
                    
                    var PredictedPosition = Ensage.Common.Extensions.UnitExtensions.InFront(target, 200);
                    //Check the mana consumed from our prediction.
                    double TempManaConsumed = (Lightning.GetAbilityData("ball_lightning_initial_mana_base") +
                                            ((Lightning.GetAbilityData("ball_lightning_initial_mana_percentage") /
                                                 100) * Owner.MaximumMana))
                                            + ((Ensage.SDK.Extensions.EntityExtensions.Distance2D(Owner,
                                                    PredictedPosition) / 100) *
                                                 (((Lightning.GetAbilityData("ball_lightning_travel_cost_percent") /
                                                    100) * Owner.MaximumMana)));
                   if (TempManaConsumed <= Owner.Mana)
                   {
                       Lightning.UseAbility(PredictedPosition);
                       await Await.Delay(
                          (int) (Lightning.FindCastPoint() + Owner.GetTurnTime(PredictedPosition) * 2250 + Game.Ping),
                           token);
                 }
             }

               else
                {
                   var PredictedPosition = target.NetworkPosition;
                    double TempManaConsumed = (Lightning.GetAbilityData("ball_lightning_initial_mana_base") +
                                             ((Lightning.GetAbilityData("ball_lightning_initial_mana_percentage") /
                                               100) * Owner.MaximumMana))
                                          + ((Ensage.SDK.Extensions.EntityExtensions.Distance2D(Owner,
                                                     PredictedPosition) / 100) *
                                             (((Lightning.GetAbilityData("ball_lightning_travel_cost_percent") /
                                                   100) * Owner.MaximumMana)));
                if (TempManaConsumed <= Owner.Mana)
                {
                     Lightning.UseAbility(PredictedPosition);
                      await Await.Delay(
                            (int) (Lightning.FindCastPoint() + Owner.GetTurnTime(PredictedPosition) * 2250 + Game.Ping),
                         token);
                  }

             }

            }

            //Vars we need before combo.
            bool HasAghanims = Owner.HasItem(ClassId.CDOTA_Item_UltimateScepter);
            float VortexCost = Vortex.GetManaCost(Vortex.Level - 1);
            float RemnantCost = Remnant.GetManaCost(Remnant.Level - 1);
            float CurrentMana = Owner.Mana;
            float TotalMana = Owner.MaximumMana;
            //there is a reason behind this; the default delay on storm ult is larger than a minimum distance travelled.
            if (target == null) return;
            
                var TargetPosition = (target.NetworkPosition - Owner.NetworkPosition).Normalized();
                TargetPosition *= 100;
                TargetPosition = target.NetworkPosition + TargetPosition;
                double ManaConsumed = (Lightning.GetAbilityData("ball_lightning_initial_mana_base") +
                                       ((Lightning.GetAbilityData("ball_lightning_initial_mana_percentage") / 100) *
                                        CurrentMana))
                                      + ((Ensage.SDK.Extensions.EntityExtensions.Distance2D(Owner, TargetPosition) /
                                          100) *
                                         (((Lightning.GetAbilityData("ball_lightning_travel_cost_percent") / 100) *
                                           CurrentMana)));
            

            //Always auto attack if we have an overload charge.
            if (UnitExtensions.HasModifier(Owner, "modifier_storm_spirit_overload") && target != null)
            {
                Owner.Attack(target);
                await Await.Delay(400);
            }

            //Vortex prioritization logic [do we have q/w enabled, do we have the mana to cast both, do they have lotus, do we have an overload modifier]
            if (!silenced && target != null && target.IsAlive &&
                !UnitExtensions.HasModifier(Owner, "modifier_storm_spirit_overload") &&
                Config.AbilityToggler.Value.IsEnabled(Vortex.Name) && Vortex.CanBeCasted()
                && Config.AbilityToggler.Value.IsEnabled(Remnant.Name) && Remnant.CanBeCasted()
                && (VortexCost + RemnantCost) <= CurrentMana)
            {
                //Use Vortex
                if (!HasAghanims)
                {
                    Vortex.UseAbility(target);
                    await Await.Delay(GetAbilityDelay(Owner, Vortex), token);
                }

                //Use Vortex differently for aghanims.
                else
                {
                    Vortex.UseAbility();
                    await Await.Delay(GetAbilityDelay(Owner, Vortex), token);
                }
            }

            //Remnant logic [w is not available, cant ult close enough for the detonation]
            if (!silenced && target != null && target.IsAlive &&
                Config.AbilityToggler.Value.IsEnabled(Remnant.Name) && Remnant.CanBeCasted()
                && !Vortex.CanBeCasted() && (CurrentMana <= RemnantCost + ManaConsumed ||
                                             Owner.Distance2D(target) <=
                                             Remnant.GetAbilityData("static_remnant_radius")))
            {
                Remnant.UseAbility();
                await Await.Delay(GetAbilityDelay(Owner, Remnant), token);
            }

            //Ult logic [nothing else is available or we are not in range for a q]
            if (!UnitExtensions.HasModifier(Owner, "modifier_storm_spirit_overload") &&
                !silenced && target != null && target.IsAlive &&
                Config.AbilityToggler.Value.IsEnabled(Lightning.Name) && Lightning.CanBeCasted()
                && (!Remnant.CanBeCasted() || Owner.Distance2D(target) >=
                    Remnant.GetAbilityData("static_remnant_radius"))
                && (!Vortex.CanBeCasted(target) || Owner.Distance2D(target) <= UltDistance))
                //todo: alternate check for aghanims
            {
                Lightning.UseAbility(TargetPosition);
                int delay = (int) ((Lightning.FindCastPoint() + Owner.GetTurnTime(TargetPosition)) * 1250.0 +
                                   Game.Ping);
                Log.Debug($"{delay}ms to wait.");
                await Task.Delay(delay);
            }

            if ((this.BloodThorn != null &&
                (this.BloodThorn.Item.IsValid &&
                target != null &&
                this.BloodThorn.Item.CanBeCasted(target) &&
                this.Config.ItemToggler.Value.IsEnabled(this.BloodThorn.Item.Name))))
            {
                Log.Debug("Using Bloodthorn");
                this.BloodThorn.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if ((this.SheepStick != null &&
                 (this.SheepStick.Item.IsValid &&
                  target != null &&
                  this.SheepStick.Item.CanBeCasted(target) &&
                  this.Config.ItemToggler.Value.IsEnabled("item_sheepstick"))))
            {
                Log.Debug("Using Sheepstick");
                this.SheepStick.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if ((this.Orchid != null) && this.Orchid.Item.IsValid && target != null &&
                (this.Orchid.Item.CanBeCasted(target) && this.Config.ItemToggler.Value.IsEnabled("item_orchid")))
            {
                Log.Debug("Using Orchid");
                this.Orchid.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if ((this.RodofAtos != null &&
               (this.RodofAtos.Item.IsValid &&
                  target != null &&
                  this.RodofAtos.Item.CanBeCasted(target) &&
                  this.Config.ItemToggler.Value.IsEnabled("item_rod_of_atos"))))
            {
                Log.Debug("Using RodofAtos");
                this.RodofAtos.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if ((this.VeilofDiscord != null &&
                 (this.VeilofDiscord.Item.IsValid &&
                  target != null &&
                  this.VeilofDiscord.Item.CanBeCasted() &&
                  this.Config.ItemToggler.Value.IsEnabled("item_veil_of_discord"))))
            {
                Log.Debug("Using VeilofDiscord");
                this.VeilofDiscord.UseAbility(target.Position);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if ((this.HurricanePike != null &&
                 (this.HurricanePike.Item.IsValid &&
                  target != null &&
                  this.HurricanePike.Item.CanBeCasted() &&
                  this.Config.ItemToggler.Value.IsEnabled("item_hurricane_pike"))))
            {
                Log.Debug("Using HurricanePike");
                this.HurricanePike.UseAbility(target);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if ((this.ShivasGuard != null &&
                 (this.ShivasGuard.Item.IsValid &&
                  target != null &&
                  this.ShivasGuard.Item.CanBeCasted() &&
                  Owner.Distance2D(target) <= 900 &&
                  this.Config.ItemToggler.Value.IsEnabled("item_shivas_guard"))))
            {
                Log.Debug("Using Shiva's Guard");
                this.ShivasGuard.UseAbility();
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if ((this.Mjollnir != null &&
                 (this.Mjollnir.Item.IsValid &&
                  target != null &&
                  this.Mjollnir.Item.CanBeCasted() &&
                  this.Config.ItemToggler.Value.IsEnabled("item_mjollnir"))))
        {
                Log.Debug("Using Mjollnir");
                this.Mjollnir.UseAbility(Owner);
                await Await.Delay(this.GetItemDelay(target) + (int) Game.Ping, token);
            }

            if (this.Orbwalker.OrbwalkTo(target))
            {
                return;
            }

            await Await.Delay(125, token);

        }

        protected float GetSpellAmp()
        {
            // spell amp
            var me = Context.Owner as Hero;
            var spellAmp = (100.0f + me.TotalIntelligence / 16.0f) / 100.0f;

            var aether = Owner.GetItemById(ItemId.item_aether_lens);
            if (aether != null)
            {
                spellAmp += aether.AbilitySpecialData.First(x => x.Name == "spell_amp").Value / 100.0f;
            }

            var talent =
                Owner.Spellbook.Spells.FirstOrDefault(
                    x => x.Level > 0 && x.Name.StartsWith("special_bonus_spell_amplify_"));

            if (talent != null)
            {
                spellAmp += talent.AbilitySpecialData.First(x => x.Name == "value").Value / 100.0f;
            }

            return spellAmp;
        }

        private IEnumerable<Unit> Remnants
            => ObjectManager.GetEntities<Unit>().Where(x => x.Name == "npc_dota_ember_spirit_remnant");

        public virtual async Task KillStealAsync(CancellationToken args)
        {
            //todo:killsteal
        }

        protected int GetAbilityDelay(Unit unit, Ability ability)
        {
            return (int) (((ability.FindCastPoint() + this.Owner.GetTurnTime(unit)) * 1000.0) + Game.Ping) + 50;
        }

        protected int GetItemDelay(Unit unit)
        {
            return (int) ((this.Owner.GetTurnTime(unit) * 1000.0) + Game.Ping) + 100;
        }

        protected int GetItemDelay(Vector3 pos)
        {
            return (int) ((this.Owner.GetTurnTime(pos) * 1000.0) + Game.Ping) + 100;
        }

        protected override void OnActivate()
        {
            this.KillStealHandler = UpdateManager.Run(this.KillStealAsync, true);

            this.Remnant = UnitExtensions.GetAbilityById(this.Owner, AbilityId.storm_spirit_static_remnant);
            this.Vortex = UnitExtensions.GetAbilityById(this.Owner, AbilityId.storm_spirit_electric_vortex);
            this.Lightning = UnitExtensions.GetAbilityById(this.Owner, AbilityId.storm_spirit_ball_lightning);



            this.context.Inventory.Attach(this);

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            this.context.Inventory.Detach(this);
        }
    }
}