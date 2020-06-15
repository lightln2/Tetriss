using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class V256Tests
{
    [TestMethod]
    public void V256_ToString()
    {
        Assert.AreEqual(
            "05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05 05",
            ((V256)5).ToString());
    }

    [TestMethod] public void V256_And() => Assert.AreEqual((V256)4, (V256)5 & (V256)6);

    [TestMethod] public void V256_Or() => Assert.AreEqual((V256)7, (V256)5 | (V256)6);

    [TestMethod] public void V256_Not() => Assert.AreEqual((V256)250, ~(V256)5);

    [TestMethod] public void V256_Eq() => Assert.AreEqual(true, (V256)222 == (V256)222);

    [TestMethod] public void V256_Neq() => Assert.AreEqual(true, (V256)222 != (V256)242);

    [TestMethod]
    public void V256_ShiftRight()
    {
        Assert.AreEqual(
            new V256(
                247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247,
                247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 247, 7),
        V256.FromByte(254) >> 5);

        V256 x = new V256(
            255, 254, 253, 252, 251, 250, 249, 248, 247, 246, 245, 244, 243, 242, 241, 240,
            239, 238, 237, 236, 235, 234, 233, 232, 231, 230, 229, 228, 227, 226, 225, 224);

        Assert.AreEqual(
            "F7 EF E7 DF D7 CF C7 BF B7 AF A7 9F 97 8F 87 7F 77 6F 67 5F 57 4F 47 3F 37 2F 27 1F 17 0F 07 07",
            (x >> 5).ToString());

        V256 y = new V256(
            0xF1, 0x18, 0x80, 0x01, 0x18, 0x80, 0x01, 0x18,
            0x80, 0x01, 0x18, 0x80, 0x01, 0x18, 0x80, 0x01,
            0x18, 0x80, 0x01, 0x18, 0x80, 0x01, 0x18, 0x80,
            0x01, 0x18, 0x80, 0x01, 0x18, 0x80, 0xFF, 0x0F);
        Assert.AreEqual(
            "01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 F8 FF 00 00",
            (y >> 12).ToString());
    }

    [TestMethod]
    public void V256_ShiftLeft()
    {
        Assert.AreEqual(
            new V256(
                192, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223,
                223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223, 223),
            V256.FromByte(254) << 5);

        V256 x = new V256(
            255, 254, 253, 252, 251, 250, 249, 248, 247, 246, 245, 244, 243, 242, 241, 240,
            239, 238, 237, 236, 235, 234, 233, 232, 231, 230, 229, 228, 227, 226, 225, 224);

        Assert.AreEqual(
            "E0 DF BF 9F 7F 5F 3F 1F FF DE BE 9E 7E 5E 3E 1E FE DD BD 9D 7D 5D 3D 1D FD DC BC 9C 7C 5C 3C 1C",
            (x << 5).ToString());


        V256 y = new V256(
            0xF1, 0x18, 0x80, 0x01, 0x18, 0x80, 0x01, 0x18,
            0x80, 0x01, 0x18, 0x80, 0x01, 0x18, 0x80, 0x01,
            0x18, 0x80, 0x01, 0x18, 0x80, 0x01, 0x18, 0x80,
            0x01, 0x18, 0x80, 0x01, 0x18, 0x80, 0xFF, 0x0F);
        Assert.AreEqual(
            "00 10 8F 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 18 80 01 F8",
            (y << 12).ToString());

    }

    [TestMethod]
    public void V256_Index() => Assert.AreEqual(7,
        new V256(
            1, 2, 3, 4, 5, 6, 7, 8,
            9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32)[6]);

    [TestMethod] public void V256_Add() => Assert.AreEqual((V256)51, (V256)240 + (V256)67);

    [TestMethod] public void V256_Subtract() => Assert.AreEqual((V256)173, (V256)240 - (V256)67);

    [TestMethod]
    public void V256_Shuffle() => Assert.AreEqual(
        new V256(
            0, 1, 8, 4, 0, 1, 8, 4, 0, 1, 8, 4, 0, 1, 8, 4,
            0, 17, 24, 20, 0, 17, 24, 20, 0, 17, 24, 20, 0, 17, 24, 20),
        new V256(
            1, 2, 3, 4, 5, 6, 7, 8,
            9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32).Shuffle(V256.FromUInt(0x030700FF)));

}
