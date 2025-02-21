using Ulko.Data.Characters;
using Ulko.Data.Timeline;
using Ulko.Persistence;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Abilities
{
    public class HeroData : ICharacterData
    {
        public string Id => Hero.id;
        public string Name => Localization.Localize(Asset.displayName);
        public CharacterSide CharacterSide => CharacterSide.Heroes;
        public List<CharacterTag> CharacterTags => Asset.tags;
        public ElementalTag Element => Asset.element;
        public int Level => PlayerProfile.GetHeroLevel(Hero.id);

        public int HP => Hero.hp;
        public float GetStat(Stat stat) => PlayerProfile.GetHeroStat(Hero.id, stat);
        public List<ActiveStatus> Statuses { get; private set; } = new List<ActiveStatus>();
        public bool HasStatus(string id) => Statuses.Find(s => s.status.id == id) != null;

        public Persistence.Hero Hero { get; private set; }
        public HeroAsset Asset { get; private set; }

        public static List<ICharacterData> AllActiveHeroes(IMilestone milestone)
        {
            var heroes = new List<ICharacterData>();
            foreach(var hero in PlayerProfile.ActiveParty)
            {
                heroes.Add(new HeroData(hero, milestone));
            }

            return heroes;
        }

        public HeroData(Persistence.Hero hero, IMilestone milestone)
        {
            Hero = hero;
            Asset = milestone.Party.Find(x => x.id == hero.id);
        }

        private void AddStatus(ActiveStatus status)
        {
            var index = Statuses.FindIndex(s => s.status.id == status.status.id);
            if (index != -1)
            {
                Statuses[index].duration = status.duration;
                Statuses[index].elapsedTime = 0;
            }
            else
            {
                var newStatus = status.Clone();
                newStatus.lastApplyTime = status.status.applyType == Status.ApplyType.OverTime ? Time.time : -1;

                Statuses.Add(newStatus);
            }
        }

        private void RemoveStatus(Status status)
        {
            Statuses.RemoveAll(s => s.status == status);
        }

        public void ApplyResult(CharacterResult result)
        {
            if (HP > 0 || result.revive)
            {
                Hero.hp = (int)Mathf.Clamp(HP + result.hpDiff, 0, GetStat(Stat.Fortitude));

                if(HP > 0)
                {
                    foreach (var status in result.addStatus)
                    {
                        AddStatus(status);
                    }
                }

                foreach (var status in result.removeStatus)
                {
                    RemoveStatus(status);
                }
            }
        }
    }

    public class AbilityAction : IActionData
    {
        public string id;
        public ICharacterData actor;
        public List<ICharacterData> targets = new List<ICharacterData>();
        public AbilityNode abilityNode;

        public ICharacterData Actor
        {
            get { return actor; }
            set { actor = value; }
        }

        public List<ICharacterData> Targets
        {
            get { return targets; }
            set { targets = value; }
        }

        public List<IEffect> Effects
        {
            get { return effects; }
            set { effects = value; }
        }
        private List<IEffect> effects = new List<IEffect>();

        public float Multiplier => 1f;

        public AbilityAction() { }

        public AbilityAction(string id, ICharacterData actor, List<ICharacterData> targets, AbilityNode abilityNode)
        {
            this.id = id;
            this.actor = actor;
            this.targets = targets;
            this.abilityNode = abilityNode;

            effects.AddRange(abilityNode.effects.effects);
        }

        public void From(AbilityAction source)
        {
            id = source.id;
            actor = source.actor;
            targets.Clear();
            targets.AddRange(source.targets);
            abilityNode = source.abilityNode;
            effects.Clear();
            foreach (var effect in source.effects)
            {
                effects.Add(effect.Clone());
            }
        }

        public IActionData Clone()
        {
            var action = new AbilityAction();
            action.From(this);

            return action;
        }

        public EffectResults Resolve(List<ICharacterData> all, List<StatModifier> statModifiers)
        {
            return EffectResult.GetResults(this, all, statModifiers, null);
        }

        public void Apply(List<ICharacterData> all)
        {
            var statModifiers = new List<StatModifier>();

            foreach(var status in actor.Statuses)
            {
                var action = new AbilityAction(status.status.id, actor, targets, status.status.node);
                action.Resolve(all, statModifiers);
            }

            var results = EffectResult.GetResults(this, all, statModifiers, null);

            foreach (var result in results.results)
            {
                foreach (var characterResult in result.characters)
                {
                    var character = all.Find(c => c.Id == characterResult.targetId);
                    if (character != null)
                    {
                        character.ApplyResult(characterResult);
                    }
                }
            }
        }
    }
}