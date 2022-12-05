using StrikeArmy.Database.Model;
using StrikeArmy.Services;

namespace Tests;

public class BoltCardTests
{
    [Fact]
    public void HappyPath()
    {
        var p = "DA5FCF957E854374A78F42409E0207DE";
        var c = "4ADA1058EA9A5939";
        var cfg = new BoltCardConfig()
        {
            K0 = "93126ee98e2d49ee949f6bdfb2a338c2".ToGuid(),
            K1 = "68177f78790b4547b43f9b1210fd7a55".ToGuid(),
            K2 = "2918e75d15eb4f67935b3e9d8ae9de4f".ToGuid(),
            K3 = "72f988b2a6fd4e3c8a079264f72e1fb4".ToGuid(),
            K4 = "78cb5cbd87e44fc09511a8f939288f11".ToGuid()
        };

        var ctr = BoltCard.CheckBoltCard(cfg, p, c);
        Assert.Equal(7u, ctr);
    }

    [Fact]
    public void SubKeyGeneration()
    {
        var (k1, k2) = BoltCard.CreateSubKey(Convert.FromHexString("2b7e151628aed2a6abf7158809cf4f3c"));
        Assert.Equal("fbeed618357133667c85e08f7236a8de", k1.ToHex());
        Assert.Equal("f7ddac306ae266ccf90bc11ee46d513b", k2.ToHex());
    }

    [Theory]
    [InlineData("2b7e151628aed2a6abf7158809cf4f3c", "", "bb1d6929e95937287fa37d129b756746")]
    [InlineData("2b7e151628aed2a6abf7158809cf4f3c", "6bc1bee22e409f96e93d7e117393172a", "070a16b46b4d4144f79bdd9dd04a287c")]
    //[InlineData("2b7e151628aed2a6abf7158809cf4f3c", "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411",
    //    "dfa66747de9ae63030ca32611497c827")]
    //[InlineData("2b7e151628aed2a6abf7158809cf4f3c",
    //    "6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710",
    //    "51f0bebf7e3b9d92fc49741779363cfe")]
    public void AesCmac(string key, string m, string cmac)
    {
        var c = BoltCard.AesCmac(Convert.FromHexString(key), Convert.FromHexString(m));
        Assert.Equal(cmac, c.ToHex());
    }
}
