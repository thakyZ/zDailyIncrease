using System.Collections.Generic;
using StardewModdingAPI.Advanced;

namespace zDailyIncrease
{
  public class SocialConfig
  {
    public bool enabled
    {
      get; set;
    } = true;

    public bool noDecrease
    {
      get; set;
    } = true;

    public bool noIncrease
    {
      get; set;
    } = false;

    public bool randomIncrease
    {
      get; set;
    } = false;

    public List<IndividualNpcConfig> individualConfigs
    {
      get; set;
    } = new List<IndividualNpcConfig>()
    {
      new IndividualNpcConfig("Default", 2, 5, 2500)
    };
  }

  public class IndividualNpcConfig
  {
    public string name
    {
      get; set;
    }
    public int baseIncrease
    {
      get; set;
    }
    public int talkIncrease
    {
      get; set;
    }
    public int max
    {
      get; set;
    }

    public IndividualNpcConfig(string name, int baseIncrease, int talkIncrease, int max)
    {
      this.name = name;
      this.baseIncrease = baseIncrease;
      this.talkIncrease = talkIncrease;
      this.max = max;
    }
  }
}
