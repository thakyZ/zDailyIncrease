using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace zDailyIncrease
{
  public class zDailyIncrease : Mod
  {
    public static SocialConfig ModConfig
    {
      get;
      private set;
    }

    public static Farmer Player => Game1.player;

    public static Game1 TheGame => Program.gamePtr;

    public Random rnd = new Random();

    public override void Entry(params object[] objects)
    {
      ModConfig = new SocialConfig();
      ModConfig = ModConfig.InitializeConfig<SocialConfig>(base.BaseConfigPath);
      TimeEvents.OnNewDay += GameEvents_OnNewDay;
      Log.Out("zDailyIncrease => Initialized");
    }

    private void GameEvents_OnNewDay(object sender, EventArgsNewDay e)
    {
      // This calculation needs to be triggered at the end of the day / before saving
      // So only perform action if e.IsNewDay = true as per SMAPI doc
      if (ModConfig.enabled && e.IsNewDay == true)
      {
        Log.SyncColour($"zDailyIncrease randomIncrease value is: {ModConfig.randomIncrease}", ConsoleColor.Red);

        Log.SyncColour($"{Environment.NewLine}Friendship increaser enabled. Starting friendship calculation.{Environment.NewLine}", ConsoleColor.Green);
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

        string[] npcNames = Player.friendships.Keys.ToArray<string>();
        foreach (string npcName in npcNames)
        {
          IndividualNpcConfig config = npcConfigsMap.ContainsKey(npcName) ? npcConfigsMap[npcName] : npcConfigsMap["Default"];
          int[] friendshipParams = Player.friendships[npcName];
          int friendshipValue = friendshipParams[0];
          Log.SyncColour($"{npcName}'s starting friendship value is {Player.getFriendshipLevelForNPC(npcName)}.", ConsoleColor.Green);
          Log.SyncColour($"{npcName}'s current heart level is {Player.getFriendshipHeartLevelForNPC(npcName)}.", ConsoleColor.Green);

          // Not sure why there's a special condition added for spouse. Disabling.
          //if ((Player.spouse != null) && npcName.Equals(Player.spouse))
          //{
          //    friendshipValue += config.baseIncrease + 20;
          //}

          if (ModConfig.randomIncrease == false)
          {
            if (Player.hasPlayerTalkedToNPC(npcName))
            {
              friendshipValue += config.talkIncrease;
              Log.SyncColour($"Talked to {npcName} today. Increasing friendship value by {config.talkIncrease}.", ConsoleColor.Green);
            }
            else
            {
              friendshipValue += config.baseIncrease;
              Log.SyncColour($"Didn't talk to {npcName} today. Increasing friendship value by {config.baseIncrease}.", ConsoleColor.Red);
            }
          }
          else
          {
            if (Player.hasPlayerTalkedToNPC(npcName))
            {
              friendshipValue += config.talkIncrease + rndNum2;
              Log.SyncColour($"Talked to {npcName} today. Increasing friendship value by {config.talkIncrease}, with random number {rndNum}.", ConsoleColor.Green);
            }
            else
            {
              friendshipValue += config.baseIncrease + rndNum2;
              Log.SyncColour($"Didn't talk to {npcName} today. Increasing friendship value by {config.baseIncrease}, with random number {rndNum}.", ConsoleColor.Red);
            }
          }

          if (friendshipValue > config.max)
          {
            friendshipValue = config.max;
          }

          Log.SyncColour($"{npcName}'s new friendship value is {friendshipValue}. Maximum permitted value is {config.max}.", ConsoleColor.Green);
          Player.friendships[npcName][0] = friendshipValue;
        }

        Log.SyncColour($"{Environment.NewLine}Finished friendship calculation.{Environment.NewLine}", ConsoleColor.Green);
      }
    }
  }
}
