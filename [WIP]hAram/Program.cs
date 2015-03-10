using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace hAram
{
    internal class Program
    {
        #region 멤버, 변수
        private static Menu config;
        private static Orbwalking.Orbwalker orb;
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;

        //private static Vector3[] buffs = { new Vector3(8922, 10, 7868), new Vector3(7473, 10, 6617), new Vector3(5929, 10, 5190), new Vector3(4751, 10, 3901)};
        private static Obj_AI_Hero Player = ObjectHandler.Player;
        private static Obj_AI_Hero target = null;
        private static Obj_AI_Hero followTarget = null;
        private static string[] Assasin = { "akali", "darius", "diana", "evelynn", "fizz", "katarina", "nidalee" };
        private static string[] ADTank = { "drmnudo", "garen", "gnar", "hecarim", "irelia", "jarvan iv", "jax", "leesin", "nasus", "olaf", "renekton", "rengar", "shyvana", "sion", "skarner", "trundle", "udyr", "volibear", "warwick", "wukong", "xinzhao", "yorick" };
        private static string[] ADCarry = { "ashe", "caitlyn", "corki", "draven", "ezreal", "gankplank", "graves", "jinx", "kogmaw", "lucian", "missfortune", "quinn", "sivir", "Thresh", "tristana", "tryndamere", "twitch", "urgot", "varus", "vayne" };
        private static string[] APTank = { "alistar", "amumu", "blitzcrank", "braum", "chogath", "leona", "malphite", "maokai", "nautilus", "rammus", "sejuani", "shen", "singed", "zac"};
        private static string[] APCarry = { "ahri", "anivia", "annie", "brand", "cassiopeia", "fiddlesticks", "galio", "gragas", "heimerdinger", "janna", "karma", "karthus", "leblanc", "lissandra", "lulu", "lux", "malzahar", "morgana", "nami", "nunu", "orianna", "ryze", "sona", "soraka", "swain", "syndra", "taric", "twistedfate", "veigar", "velkoz", "viktor", "xerath", "ziggs", "zilean", "zyra" };
        private static string[] APHybrid = { "kayle", "teemo" };
        private static string[] Bruiser = { "khazix", "pantheon", "riven", "talon", "vi", "yasuo", "zed" };
        private static string[] ADCaster = { "aatrox", "fiora", "jayce", "nocturne", "poppy"};
        private static string[] APOther = { "elise", "kennen", "mordekaiser", "rumble", "vladimir" };
        private static int[] Shoplist;
        private static List<int> lstHasItem = new List<int>();
        private static int lastShopID = -1;
        private static int heroType = 0;
        private static long lastFollow = 0;
        private static long followDelay = 6000000;
        private static Vector3 lastFollowTargetPos = new Vector3();
        private static long lastFollowTarget = 0;
        private static long nextFollowTargetDelay = 300000000;
        private static string status = "Follow";
        private static List<Obj_AI_Turret> lstTurrets = new List<Obj_AI_Turret>();
        private static Obj_AI_Turret turret = null;

        private static SpellDataInst qData = ObjectHandler.Player.Spellbook.GetSpell(SpellSlot.Q);
        private static SpellDataInst wData = ObjectHandler.Player.Spellbook.GetSpell(SpellSlot.W);
        private static SpellDataInst eData = ObjectHandler.Player.Spellbook.GetSpell(SpellSlot.E);
        private static SpellDataInst rData = ObjectHandler.Player.Spellbook.GetSpell(SpellSlot.R);
        private static bool qGapFlag = false;
        private static bool wGapFlag = false;
        private static bool eGapFlag = false;
        private static bool rGapFlag = false;
        #endregion

        #region 초기화
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Loaded hAram");
            InitMenu();
            InitPlayer();
            InitGapCloser();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private static void InitMenu()
        {
            config = new Menu("hAram", "hAram", true);
            config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            orb = new Orbwalking.Orbwalker(config);
            config.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            //config.AddSubMenu()
            config.AddToMainMenu();
        }

        private static void InitPlayer()
        {
            if (ADCarry.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 1;
                int[] shoplist = { 3006, 1042, 3086, 3087, 3144, 3153, 1038, 3181, 1037, 3035, 3026, 0 };
                Shoplist = shoplist;
            }
            else if (ADTank.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 2;
                int[] shoplist = { 3047, 1011, 3134, 3068, 3024, 3025, 3071, 3082, 3143, 3005, 0 };
                Shoplist = shoplist;
            }
            else if (APTank.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 3;
                int[] shoplist = { 3111, 1031, 3068, 1057, 3116, 1026, 3001, 3082, 3110, 3102, 0 };
                Shoplist = shoplist;
            }
            else if (APHybrid.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 4;
                int[] shoplist = { 1001, 3108, 3115, 3020, 1026, 3136, 3089, 1043, 3091, 3151, 3116 };
                Shoplist = shoplist;
            }
            else if (Bruiser.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 5;
                int[] shoplist = { 3111, 3134, 1038, 3181, 3155, 3071, 1053, 3077, 3074, 3156, 3190 };
                Shoplist = shoplist;
            }
            else if (Assasin.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 6;
                int[] shoplist = { 3020, 3057, 3100, 1026, 3089, 3136, 3151, 1058, 3157, 3135, 0 };
                Shoplist = shoplist;
            }
            else if (APCarry.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 7;
                int[] shoplist = { 3028, 1001, 3020, 3136, 1058, 3089, 3174, 3151, 1026, 3001, 3135, 0 };
                Shoplist = shoplist;
            }
            else if (APOther.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 8;
                int[] shoplist = { 3145, 3020, 3152, 1026, 3116, 1058, 3089, 1026, 3001, 3157 };
                Shoplist = shoplist;
            }
            else if (ADCaster.Contains(Player.ChampionName.ToLower()))
            {
                heroType = 9;
                int[] shoplist = { 3111, 3044, 3086, 3078, 3144, 3153, 3067, 3065, 3134, 3071, 3156, 0 };
                Shoplist = shoplist;
            }
            else
            {
                int[] shoplist = { 3111, 3044, 3086, 3078, 3144, 3153, 3067, 3065, 3134, 3071, 3156, 0 };
                Shoplist = shoplist;
            }


            Q = new Spell(SpellSlot.Q, GetSpellRange(qData));
            Q.Speed = qData.SData.MissileSpeed;
            Q.Width = qData.SData.LineWidth;
            Q.Delay = qData.SData.SpellCastTime;

            W = new Spell(SpellSlot.W, GetSpellRange(wData));
            W.Speed = wData.SData.MissileSpeed;
            W.Width = wData.SData.LineWidth;


            E = new Spell(SpellSlot.E, GetSpellRange(eData));
            E.Speed = eData.SData.MissileSpeed;
            E.Width = eData.SData.LineWidth;

            R = new Spell(SpellSlot.R, GetSpellRange(rData));
            R.Speed = rData.SData.MissileSpeed;
            R.Width = rData.SData.LineWidth;
        }

        private static void InitGapCloser()
        {
            
            foreach (Gapcloser gap in AntiGapcloser.Spells)
            {
                if (gap.SpellName.Equals(qData.SData.Name))
                    qGapFlag = true;
                
                if (gap.SpellName.Equals(wData.SData.Name))
                    wGapFlag = true;

                if (gap.SpellName.Equals(eData.SData.Name))
                    eGapFlag = true;

                if (gap.SpellName.Equals(rData.SData.Name))
                    rGapFlag = true;
            }
        }
        #endregion

        #region 이벤트
        private static void Game_OnGameUpdate(EventArgs args)
        {
            bool enabled = config.Item("Enabled").GetValue<bool>();
            if (!Player.IsDead && enabled)
            {
                if (Player.HealthPercentage() <= 20)
                    AntiGapclose();

                SetAttack();
                BuyItems();
                CastSpells();
                Following();
                AutoLevel();
                GetBuffs();
            }
            else
                RefreshLastShop();
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && target is Obj_AI_Hero)
            {
                status = "Fight";

                float distance1 = 0;
                if (Player.Distance(target) <= Player.AttackRange - 120)
                {
                    distance1 = Player.AttackRange - Player.Distance(target) - 10;
                    if (Player.Team == GameObjectTeam.Chaos)
                        Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(Player.Position.X + distance1, Player.Position.Y, Player.Position.Z));
                    else
                        Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(Player.Position.X - distance1, Player.Position.Y, Player.Position.Z));
                }
                else if (Player.Distance(target) >= Player.AttackRange - 50)
                {
                    distance1 = Player.AttackRange - Player.Distance(target) - 10;
                    if (Player.Team == GameObjectTeam.Chaos)
                        Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(Player.Position.X - distance1, Player.Position.Y, Player.Position.Z));
                    else
                        Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(Player.Position.X + distance1, Player.Position.Y, Player.Position.Z));
                }
            }
        }
        #endregion

        #region 사용자함수

        private static void SetAttack()
        {
            target = null;
            Obj_AI_Minion minionTarget = null;
            AttackableUnit orbTarget = orb.GetTarget();


            if (orbTarget != null)
            {
                if (orbTarget is Obj_AI_Hero)
                {
                    status = "Fight";
                    target = (Obj_AI_Hero)orbTarget;
                }
                else if (orbTarget is Obj_AI_Minion)
                {
                    status = "Follow";
                    minionTarget = (Obj_AI_Minion)orbTarget;
                }
            }

            if (target != null)
            {
                status = "Fight";
                orb.SetAttack(true);
                orb.InAutoAttackRange(target);
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            else if (minionTarget != null)
            {
                status = "Follow";
                if (target.Health <= Player.GetAutoAttackDamage(Player, true))
                {
                    orb.SetAttack(true);
                    orb.InAutoAttackRange(target);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }
            else
            {
                status = "Follow";
                orb.SetAttack(true);
                orb.InAutoAttackRange(orbTarget);
                Player.IssueOrder(GameObjectOrder.AttackUnit, orbTarget);
            }



            lstTurrets = ObjectHandler.Get<Obj_AI_Turret>().Enemies.ToList().FindAll(t => !t.IsDead);
            turret = lstTurrets.OrderBy(t => t.Distance(Player)).ToList().Count > 0 ? lstTurrets.OrderBy(t => t.Distance(Player)).ToList()[0] : null;

            if (turret != null)
            {
                if (turret.Distance(Player) <= Player.AttackRange)
                {
                    orb.InAutoAttackRange(turret);
                    orb.SetAttack(true);
                }
                
            }
        }

        private static Obj_AI_Hero GetFollowTarget(Obj_AI_Hero exceptHero)
        {
            Obj_AI_Hero targett = null;
            List<Obj_AI_Hero> lstAlies = ObjectHandler.Get<Obj_AI_Hero>().Allies;
            bool lessRangeHero = false;

            if (exceptHero != null)
            {
                foreach (Obj_AI_Hero hero in lstAlies)
                {
                    if (!hero.IsDead
                        && !hero.InFountain()
                        && !hero.IsMe
                        && hero.HealthPercentage() >= 25
                        && !hero.ChampionName.Equals(exceptHero.ChampionName))
                    {
                        if (Player.AttackRange > hero.AttackRange)
                        {
                            lessRangeHero = true;
                            break;

                        }
                    }
                }

                foreach (Obj_AI_Hero hero in lstAlies)
                {
                    if (lessRangeHero)
                    {
                        if (!hero.IsDead
                        && !hero.InFountain()
                        && !hero.IsMe
                        && hero.HealthPercentage() >= 25
                        && !hero.ChampionName.Equals(exceptHero.ChampionName))
                        {
                            if (Player.AttackRange > hero.AttackRange)
                            {
                                targett = hero;
                                lastFollowTarget = DateTime.Now.Ticks;
                                lastFollowTargetPos = targett.Position;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (!hero.IsDead
                        && !hero.InFountain()
                        && !hero.IsMe
                        && hero.HealthPercentage() >= 25
                        && !hero.ChampionName.Equals(exceptHero.ChampionName))
                        {
                            targett = hero;
                            lastFollowTarget = DateTime.Now.Ticks;
                            lastFollowTargetPos = targett.Position;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (Obj_AI_Hero hero in lstAlies)
                {
                    if (!hero.IsDead
                        && !hero.InFountain()
                        && !hero.IsMe
                        && hero.HealthPercentage() >= 25)
                    {
                        if (Player.AttackRange > hero.AttackRange)
                        {
                            lessRangeHero = true;
                            break;

                        }
                    }
                }

                foreach (Obj_AI_Hero hero in lstAlies)
                {
                    if (lessRangeHero)
                    {
                        if (!hero.IsDead
                        && !hero.InFountain()
                        && !hero.IsMe
                        && hero.HealthPercentage() >= 25)
                        {
                            if (Player.AttackRange > hero.AttackRange)
                            {
                                targett = hero;
                                lastFollowTarget = DateTime.Now.Ticks;
                                lastFollowTargetPos = targett.Position;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (!hero.IsDead
                        && !hero.InFountain()
                        && !hero.IsMe
                        && hero.HealthPercentage() >= 25)
                        {
                            targett = hero;
                            lastFollowTarget = DateTime.Now.Ticks;
                            lastFollowTargetPos = targett.Position;
                            break;
                        }
                    }
                }
            }
            return targett;
        }

        private static void Following()
        {

            if ((DateTime.Now.Ticks - lastFollowTarget > nextFollowTargetDelay)
                || followTarget == null
                || followTarget.IsDead
                || followTarget.HealthPercentage() < 25)
            {
                followTarget = GetFollowTarget(followTarget);
            }

            if (followTarget != null)
            {
                if (status != "GetBuff" && status != "Fight" && (DateTime.Now.Ticks - lastFollow > followDelay))
                {
                    Random r = new Random();
                    int distance1 = r.Next(100, 300);
                    int distance2 = r.Next(100, 300);

                    if (Player.AttackRange >= followTarget.AttackRange)
                    {
                        if (Player.Team == GameObjectTeam.Chaos)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(followTarget.Position.X + distance1, followTarget.Position.Y, followTarget.Position.Z + distance2));
                            orb.SetOrbwalkingPoint(new Vector3(followTarget.Position.X + distance1, followTarget.Position.Y, followTarget.Position.Z + distance2));
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(followTarget.Position.X - distance1, followTarget.Position.Y, followTarget.Position.Z - distance2));
                            orb.SetOrbwalkingPoint(new Vector3(followTarget.Position.X - distance1, followTarget.Position.Y, followTarget.Position.Z - distance2));
                        }
                    }
                    else
                    {
                        if (Player.Team == GameObjectTeam.Order)
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(followTarget.Position.X + distance1, followTarget.Position.Y, followTarget.Position.Z + distance2));
                            orb.SetOrbwalkingPoint(new Vector3(followTarget.Position.X + distance1, followTarget.Position.Y, followTarget.Position.Z + distance2));
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(followTarget.Position.X - distance1, followTarget.Position.Y, followTarget.Position.Z - distance2));
                            orb.SetOrbwalkingPoint(new Vector3(followTarget.Position.X - distance1, followTarget.Position.Y, followTarget.Position.Z - distance2));
                        }
                    }
                    lastFollow = DateTime.Now.Ticks;
                }
            }
        }

        private static void BuyItems()
        {
            if (Player.InFountain())
            {
                for (int i = 0; i < Shoplist.Length; i++)
                {
                    if (!lstHasItem.Contains(Shoplist[i]))
                    {
                        Items.Item Item = new Items.Item(Shoplist[i]);
                        Item.Buy();

                        InventorySlot[] slots = Player.InventoryItems;
                        for (int j = 0; j < slots.Length; j++)
                        {
                            if (slots[j].IsValidSlot()
                                && slots[j].Id != null
                                && slots[j].Id != 0
                                && Items.HasItem(Shoplist[i])
                                && !lstHasItem.Contains(Shoplist[i]))
                            {
                                lstHasItem.Add(Shoplist[i]);
                                break;
                            }
                        }
                        
                    }
                }
            }
        }

        private static void CastSpells()
        {
            CastSpell(E, eData, eGapFlag);
            CastSpell(Q, qData, qGapFlag);
            CastSpell(W, wData, wGapFlag);
            CastSpell(R, rData, rGapFlag);
        }

        private static void CastSpell(Spell spell, SpellDataInst sDataInst, bool gapFlag)
        {
            target = null;
            if (heroType == 2 || heroType == 3 || heroType == 5 || heroType == 6 || heroType == 9)
                TargetSelector.Mode = TargetSelector.TargetingMode.AutoPriority;
            else if (heroType == 4 || heroType == 7 || heroType == 8)
                TargetSelector.Mode = TargetSelector.TargetingMode.LessCast;
            else
                TargetSelector.Mode = TargetSelector.TargetingMode.LessAttack;


            if (heroType == 3 || heroType == 4 || heroType == 6 || heroType == 7 || heroType == 8)
                target = TargetSelector.GetTarget(spell.Range, TargetSelector.DamageType.Magical);
            else
                target = TargetSelector.GetTarget(spell.Range, TargetSelector.DamageType.Physical);
            
            if (spell.Slot != SpellSlot.R)
            {
                if (gapFlag && (heroType == 1 | heroType == 8) && !spell.IsKillable(target))
                    return;

                if (target != null && spell.IsReady())
                {
                    var pred = spell.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.Medium)
                    {
                        if (sDataInst.SData.IsToggleSpell)
                        {
                            if (spell.Instance.ToggleState == 1)
                                spell.Cast();
                        }
                        else
                        {
                            if (spell.IsReady())
                            {
                                if (sDataInst.SData.TargettingType == 0)
                                    spell.Cast();
                                else if (sDataInst.SData.TargettingType == 1)
                                    spell.CastOnUnit(target);
                                else
                                    spell.Cast(pred.CastPosition);
                            }
                        }
                    }

                }
            }
            else
            {
                if (sDataInst.SData.TargettingType == 0 && Player.HealthPercentage() <= 55)
                    spell.Cast();

                var pred = spell.GetPrediction(target);
                if (target != null && spell.IsReady() && (spell.IsKillable(target) 
                    || (heroType == 1 && status == "Fight") 
                    || (Player.HealthPercentage() <= 30 && status == "Fight")))
                {
                    if (pred.Hitchance >= HitChance.Medium)
                    {
                        if (sDataInst.SData.IsToggleSpell)
                        {
                            if (spell.Instance.ToggleState == 1)
                                spell.Cast();
                        }
                        else
                        {
                            if (sDataInst.SData.TargettingType == 1)
                                spell.CastOnUnit(target);
                            else
                                spell.Cast(pred.CastPosition);
                        }
                    }
                }
                else if (target != null && spell.IsReady() 
                    && (heroType == 2 || heroType == 3 || heroType == 5))
                {
                    if (Player.HealthPercentage() <= 40)
                    {
                        if (sDataInst.SData.IsToggleSpell)
                        {
                            if (spell.Instance.ToggleState == 1)
                                spell.Cast();
                        }
                        else
                        {
                            if (sDataInst.SData.TargettingType == 1)
                                spell.CastOnUnit(target);
                            else if (pred.Hitchance >= HitChance.VeryHigh)
                                spell.Cast(pred.CastPosition);
                        }
                    }
                }
                else if (target != null && R.IsReady())
                {
                    if (sDataInst.SData.TargettingType == 0 && target.HealthPercentage() < 70)
                        spell.Cast();
                }
            }
        }

        private static void AntiGapclose()
        {
            Spell spell = null;
            if (qGapFlag && Q.IsReady() && qData.SData.TargettingType == 7)
                spell = Q;

            if (wGapFlag && W.IsReady() && wData.SData.TargettingType == 7)
                spell = W;
            if (eGapFlag && E.IsReady() && eData.SData.TargettingType == 7)
                spell = E;
            if (rGapFlag && R.IsReady() && rData.SData.TargettingType == 7)
                spell = R;

            if (spell != null)
            {
                if (spell.IsReady())
                {
                    if (Player.Team == GameObjectTeam.Chaos)
                        spell.Cast(new Vector3(Player.Position.X + spell.Range, Player.Position.Y, Player.Position.Z + spell.Range));
                    else
                        spell.Cast(new Vector3(Player.Position.X - spell.Range, Player.Position.Y, Player.Position.Z - spell.Range));   
                }
            }
        }

        private static void RefreshLastShop()
        {
            InventorySlot[] slots = Player.InventoryItems;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsValidSlot()
                    && slots[i].Id != null 
                    && slots[i].Id != 0)
                {
                    for (int j = lastShopID + 1; j < Shoplist.Length; j++)
                    {
                        if (Items.HasItem(Shoplist[j])
                            && !lstHasItem.Contains(Shoplist[j]))
                                lstHasItem.Add(Shoplist[j]);   
                    }
                } 
            }
            
        }

        public static float GetSpellRange(SpellDataInst targetSpell, bool IsChargedSkill = false)
        {
            if (targetSpell.SData.CastRangeDisplayOverride <= 50)
            {
                if (targetSpell.SData.CastRange <= 50)
                {
                    return targetSpell.SData.CastRadius;
                }
                else
                {
                    if (!IsChargedSkill)
                        return targetSpell.SData.CastRange;
                    else
                        return targetSpell.SData.CastRadius;
                }
            }
            else
                return Player.ChampionName.ToLowerInvariant() == "urgot" ? targetSpell.SData.CastRange : targetSpell.SData.CastRangeDisplayOverride;
        }

        private static void GetBuffs()
        {
            //var lstHealth = ObjectHandler.Get<Obj_AI_Base>().FindAll(health => health.Name.Contains("HA_AP_HealthRelic")).ToList().OrderBy(health => Player.Distance(health, true)).ToList();
            //Obj_AI_Base healthBuff = null;

            //if (lstHealth.Count > 0)
            //{
            //    healthBuff = lstHealth[0];
            //}
            //target = null;
            //target = TargetSelector.GetTarget(Player.AttackRange, TargetSelector.DamageType.Physical);

            //if (target == null && healthBuff != null)
            //{
            //    if (Player.HealthPercentage() <= 50 && Player.Distance(healthBuff.Position) > 50)
            //    {
            //        Console.WriteLine(Player.Distance(healthBuff.Position));
            //        status = "GetBuff";
            //        Player.IssueOrder(GameObjectOrder.MoveTo, healthBuff.Position);
            //    }
            //    else
            //    {
            //        status = "Follow";
            //    }
            //}
        }

        private static void AutoLevel()
        {
            if ((Q.Level + W.Level + E.Level + R.Level) < Player.Level)
            {
                int rLevel = 0;

                switch (Player.Level)
                {
                    case 6:
                        rLevel = 1;
                        break;
                    case 11:
                        rLevel = 2;
                        break;
                    case 16:
                        rLevel = 3;
                        break;
                }

                if (R.Level < Q.Level && R.Level != rLevel)
                    Player.Spellbook.LevelSpell(SpellSlot.R);
                if ((Q.Level <= E.Level && Q.Level != 5) || (Q.Level == 0))
                    Player.Spellbook.LevelSpell(SpellSlot.Q);
                else if ((E.Level <= W.Level && E.Level != 5) || (E.Level == 0))
                    Player.Spellbook.LevelSpell(SpellSlot.E);
                else
                    Player.Spellbook.LevelSpell(SpellSlot.W);
            }
        }
        #endregion
    }
}

