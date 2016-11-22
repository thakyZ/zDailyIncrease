using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace zDailyIncrease
{
  public class zDailyIncrease : Mod
  {
    public SocialConfig ModConfig
    {
      get;
      private set;
    }

    public Farmer Player => Game1.player;

    public Game1 TheGame => Program.gamePtr;

    public Random rnd = new Random();

    public Dictionary<string, int> prevFriends;

    public override void Entry(IModHelper helper)
    {
      SocialConfig ModConfig = helper.ReadConfig<SocialConfig>();
      GameEvents.OneSecondTick += new EventHandler(GameEvents_OneSecondTick);
      TimeEvents.OnNewDay += new EventHandler<EventArgsNewDay>(GameEvents_OnNewDay);
      Monitor.Log("zDailyIncrease => Initialized", LogLevel.Debug);
      // This calculation needs to be triggered at the end of the day / before saving
    }

    private void GameEvents_OneSecondTick(object sender, EventArgs e)
    {
      if (!Game1.hasLoadedGame)
      {
        return;
      }
      OneSecondUpdate();
    }

    private void GameEvents_OnNewDay(object sender, EventArgsNewDay e)
    {
      if (!Game1.hasLoadedGame)
      {
        return;
      }
      OnNewDay(sender, e);
    }

    private void OneSecondUpdate()
    {
      if (ModConfig.enabled && ModConfig.noDecrease)
      {
        if (prevFriends == null)
        {
          SerializableDictionary<string, int[]> serializableDictionary = Player.friendships;
          //if (serializableDictionary != null)
          //{
          //  serializableDictionary. = (KeyValuePair<string, int[]> p) => p.Key.ToString();
          //}
          prevFriends = serializableDictionary.ToDictionary((KeyValuePair<string, int[]> p) => p.Key.ToString(), (KeyValuePair<string, int[]> p) => p.Value[0]);
        }
        foreach (KeyValuePair<string, int[]> friendship in Player.friendships)
        {
          foreach (KeyValuePair<string, int> prevFriend in prevFriends)
          {
            if (!friendship.Key.Equals(prevFriend.Key) || friendship.Value[0] >= prevFriend.Value)
            {
              continue;
            }
            friendship.Value[0] = prevFriend.Value;
          }
        }
        SerializableDictionary<string, int[]> serializableDictionary1 = Player.friendships;
        //if (C == null)
        //{
        //  Cheats.<> c.<> 9__9_3 = (KeyValuePair<string, int[]> p) => p.Key.ToString();
        //}
        prevFriends = serializableDictionary1.ToDictionary((KeyValuePair<string, int[]> p) => p.Key.ToString(), (KeyValuePair<string, int[]> p) => p.Value[0]);
      }
    }

    private void OnNewDay(object sender, EventArgsNewDay e)
    {
      // So only perform action if e.IsNewDay = true as per SMAPI doc
      if (ModConfig.enabled && e.IsNewDay == true)
      {
        Monitor.Log($"zDailyIncrease randomIncrease value is: {ModConfig.randomIncrease}", LogLevel.Trace);

        Monitor.Log($"{Environment.NewLine}Friendship increaser enabled. Starting friendship calculation.{Environment.NewLine}", LogLevel.Info);
        List<IndividualNpcConfig> individualNpcConfigs = ModConfig.individualConfigs;
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

        string[] npcNames = Player.friendships.Keys.ToArray();
        foreach (string npcName in npcNames)
        {
          IndividualNpcConfig config = npcConfigsMap.ContainsKey(npcName) ? npcConfigsMap[npcName] : npcConfigsMap["Default"];
          int[] friendshipParams = Player.friendships[npcName];
          int friendshipValue = friendshipParams[0];
          Monitor.Log($"{npcName}'s starting friendship value is {Player.getFriendshipLevelForNPC(npcName)}.", LogLevel.Trace);
          Monitor.Log($"{npcName}'s current heart level is {Player.getFriendshipHeartLevelForNPC(npcName)}.", LogLevel.Trace);

          // Not sure why there's a special condition added for spouse. Disabling.
          //if ((Player.spouse != null) && npcName.Equals(Player.spouse))
          //{
          //    friendshipValue += config.baseIncrease + 20;
          //}
          if (ModConfig.noDecrease)
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
                Monitor.Log($"Talked to {npcName} today. Increasing friendship value by {config.talkIncrease}.", LogLevel.Trace);
              }
              else
              {
                friendshipValue += config.baseIncrease;
                Monitor.Log($"Didn't talk to {npcName} today. Increasing friendship value by {config.baseIncrease}.", LogLevel.Trace);
              }
            }
            else
            {
              if (Player.hasPlayerTalkedToNPC(npcName))
              {
                friendshipValue += config.talkIncrease + rndNum2;
                Monitor.Log($"Talked to {npcName} today. Increasing friendship value by {config.talkIncrease}, with random number {rndNum}.", LogLevel.Trace);
              }
              else
              {
                friendshipValue += config.baseIncrease + rndNum2;
                Monitor.Log($"Didn't talk to {npcName} today. Increasing friendship value by {config.baseIncrease}, with random number {rndNum}.", LogLevel.Trace);
              }
            }
          }

          if (friendshipValue > config.max)
          {
            friendshipValue = config.max;
          }

          Monitor.Log($"{npcName}'s new friendship value is {friendshipValue}. Maximum permitted value is {config.max}.", LogLevel.Debug);
          Player.friendships[npcName][0] = friendshipValue;
        }

        Monitor.Log($"{Environment.NewLine}Finished friendship calculation.{Environment.NewLine}", LogLevel.Info);
      }
    }
  }
}
