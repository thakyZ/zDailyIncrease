using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Netcode;
using StardewValley.Network;

namespace zDailyIncrease
{
  public class ModEntry : Mod
  {
    public SocialConfig ModConfig { get; private set; }

    public StardewValley.Farmer Player => Game1.player;
    public NetRootDictionary<long, Farmer> OtherPlayers => Game1.otherFarmers;

    public Random rnd = new Random();

    public Dictionary<string, Friendship> prevFriends = new Dictionary<string, Friendship>();

    public override void Entry(IModHelper helper)
    {
      ModConfig = helper.ReadConfig<SocialConfig>();
      GameEvents.OneSecondTick += GameEvents_OneSecondTick;
      TimeEvents.AfterDayStarted += OnNewDay;
      Monitor.Log("zDailyIncrease => Initialized");
    }

    private void GameEvents_OneSecondTick(object sender, EventArgs e)
    {
      if (!Game1.hasLoadedGame)
      {
        return;
      }
      OneSecondUpdate();
    }

    private void OneSecondUpdate()
    {
      if (ModConfig.enabled && ModConfig.noDecrease)
      {
        if (prevFriends == null)
        {
          NetStringDictionary<Friendship, NetRef<Friendship>> serializableDictionary = Player.friendshipData;
          prevFriends = serializableDictionary.ToDictionary(p => p.Keys.ToString(), p => p[p.Keys.ToString()]);
        }
        foreach (string friendship in Player.friendshipData.Keys.ToArray())
        {
          foreach (string prevFriend in prevFriends.Keys)
          {
            if (!Player.friendshipData[friendship].Equals(prevFriend) || Player.friendshipData[friendship].Points >= prevFriends[prevFriend].Points)
            {
              continue;
            }
            Player.friendshipData[friendship].Points = prevFriends[prevFriend].Points;
          }
        }
        NetStringDictionary<Friendship, NetRef<Friendship>> serializableDictionary1 = Player.friendshipData;
        prevFriends = serializableDictionary1.ToDictionary(p => p.Keys, p => p.Values);
      }
    }

    private void OnNewDay(object sender, EventArgs e)
    {
      if (!ModConfig.disableAllOutput)
      {
        Monitor.Log($"zDailyIncrease randomIncrease value is: {ModConfig.randomIncrease}", LogLevel.Trace);
      }

      if (!ModConfig.disableAllOutput)
      {
        Monitor.Log($"{Environment.NewLine}Friendship increaser enabled. Starting friendship calculation.{Environment.NewLine}", LogLevel.Info);
      }
      IndividualNpcConfig[] individualNpcConfigs = ModConfig.individualConfigs;
      SortedDictionary<string, IndividualNpcConfig> npcConfigsMap = new SortedDictionary<string, IndividualNpcConfig>();

      foreach (IndividualNpcConfig individualNpcConfig in individualNpcConfigs)
      {
        npcConfigsMap.Add(individualNpcConfig.name, individualNpcConfig);
      }

      // Add default configuration if it's not found in the configuration file
      if (!npcConfigsMap.ContainsKey("Default"))
      {
        npcConfigsMap.Add("Default", new IndividualNpcConfig("Default", 2, 0, 2500));
      }

      int rndNum = rnd.Next(0, 10);
      float rndNum1 = rndNum * Player.LuckLevel;
      int rndNum2 = (int)rndNum1 + rndNum;

      string[] npcNames = Player.friendshipData.Keys.ToArray();
      foreach (string npcName in npcNames)
      {
        IndividualNpcConfig config = npcConfigsMap.ContainsKey(npcName) ? npcConfigsMap[npcName] : npcConfigsMap["Default"];
        Player.friendshipData.TryGetValue(npcName, out Friendship npcFriendship);
        int friendshipValue = npcFriendship.Points;
        if (!ModConfig.disableAllOutput)
        {
          Monitor.Log($"{npcName}'s starting friendship value is {Player.getFriendshipLevelForNPC(npcName)}.", LogLevel.Trace);
          Monitor.Log($"{npcName}'s current heart level is {Player.getFriendshipHeartLevelForNPC(npcName)}.", LogLevel.Trace);
        }

        if ((Player.spouse != null) && npcName.Equals(Player.spouse))
        {
          config.max += 1000;
        }

        if (ModConfig.noDecrease && !ModConfig.disableAllOutput)
        {
          Monitor.Log($"No Decrease for: {npcName}. Value is {Player.getFriendshipLevelForNPC(npcName)}", LogLevel.Trace);
        }
        if (!ModConfig.noIncrease)
        {
          if (ModConfig.randomIncrease == false)
          {
            if (Player.hasPlayerTalkedToNPC(npcName))
            {
              friendshipValue += config.talkIncrease;
              if (!ModConfig.disableAllOutput)
              {
                Monitor.Log($"Talked to {npcName} today. Increasing friendship value by {config.talkIncrease}.", LogLevel.Debug);
              }
            }
            else
            {
              friendshipValue += config.baseIncrease;
              if (!ModConfig.disableAllOutput)
              {
                Monitor.Log($"Didn't talk to {npcName} today. Increasing friendship value by {config.baseIncrease}.", LogLevel.Debug);
              }
            }
          }
          else
          {
            if (Player.hasPlayerTalkedToNPC(npcName))
            {
              friendshipValue += config.talkIncrease + rndNum2;
              if (!ModConfig.disableAllOutput)
              {
                Monitor.Log($"Talked to {npcName} today. Increasing friendship value by {config.talkIncrease}, with random number {rndNum}.", LogLevel.Debug);
              }
            }
            else
            {
              friendshipValue += config.baseIncrease + rndNum2;
              if (!ModConfig.disableAllOutput)
              {
                Monitor.Log($"Didn't talk to {npcName} today. Increasing friendship value by {config.baseIncrease}, with random number {rndNum}.", LogLevel.Debug);
              }
            }
          }
        }

        if (friendshipValue > config.max)
        {
          friendshipValue = config.max;
        }

        if (!ModConfig.disableAllOutput)
        {
          Monitor.Log($"{npcName}'s new friendship value is {friendshipValue}. Maximum permitted value is {config.max}.", LogLevel.Debug);
        }
        Player.friendshipData[npcName].Points = friendshipValue;
      }

      if (!ModConfig.disableAllOutput)
      {
        Monitor.Log($"{Environment.NewLine}Finished friendship calculation.{Environment.NewLine}", LogLevel.Debug);
      }
    }
  }
}
