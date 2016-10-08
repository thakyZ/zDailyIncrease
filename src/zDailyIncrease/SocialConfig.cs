using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace zDailyIncrease
{
  public class SocialConfig : Config
  {
    public bool enabled
    {
      get; set;
    }

    public bool randomIncrease
    {
      get; set;
    }

    public List<IndividualNpcConfig> individualConfigs
    {
      get; set;
    }

    public override T GenerateDefaultConfig<T>()
    {
      this.enabled = true;
      this.randomIncrease = false;
      this.individualConfigs = new List<IndividualNpcConfig>();
      individualConfigs.Add(new IndividualNpcConfig("Default", 2, 5, 2500));

      return (this as T);
    }
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
