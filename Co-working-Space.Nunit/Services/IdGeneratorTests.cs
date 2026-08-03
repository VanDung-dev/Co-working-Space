using Co_working_Space.Services;

namespace Co_working_Space.Nunit.Services;

[TestFixture]
public class IdGeneratorTests
{
    [Test]
    public void Next_BookingPrefix_IncludesDate()
    {
        var id = IdGenerator.Next(IdGenerator.Booking);
        Assert.That(id, Does.Match(@"^BKG-\d{8}-\d{6}-\d{3}$"));
    }

    [Test]
    public void Next_UserPrefix_ReturnsSequential()
    {
        var id1 = IdGenerator.Next(IdGenerator.User);
        var id2 = IdGenerator.Next(IdGenerator.User);
        Assert.That(id2, Is.GreaterThan(id1));
    }

    [Test]
    public void Next_DifferentPrefixes_HaveIndependentCounters()
    {
        var user = IdGenerator.Next(IdGenerator.User);
        var staff = IdGenerator.Next(IdGenerator.Staff);
        Assert.That(user, Does.StartWith("USR-"));
        Assert.That(staff, Does.StartWith("STF-"));
    }

    [Test]
    public void Next_RoomPrefix_ByCapacity()
    {
        var small = IdGenerator.Next(IdGenerator.RoomSmall);
        var medium = IdGenerator.Next(IdGenerator.RoomMedium);
        var large = IdGenerator.Next(IdGenerator.RoomLarge);
        var vip = IdGenerator.Next(IdGenerator.RoomVip);
        Assert.Multiple(() =>
        {
            Assert.That(small, Does.StartWith("RM-S-"));
            Assert.That(medium, Does.StartWith("RM-M-"));
            Assert.That(large, Does.StartWith("RM-L-"));
            Assert.That(vip, Does.StartWith("RM-V-"));
        });
    }

    [Test]
    public void Next_EquipmentPrefix_ByType()
    {
        Assert.Multiple(() =>
        {
            Assert.That(IdGenerator.Next(IdGenerator.EquipProjector), Does.StartWith("EQ-PROJ-"));
            Assert.That(IdGenerator.Next(IdGenerator.EquipTV), Does.StartWith("EQ-TV-"));
            Assert.That(IdGenerator.Next(IdGenerator.EquipMicrophone), Does.StartWith("EQ-MIC-"));
            Assert.That(IdGenerator.Next(IdGenerator.EquipWhiteboard), Does.StartWith("EQ-WB-"));
            Assert.That(IdGenerator.Next(IdGenerator.EquipSpeaker), Does.StartWith("EQ-SPK-"));
            Assert.That(IdGenerator.Next(IdGenerator.EquipCamera), Does.StartWith("EQ-CAM-"));
            Assert.That(IdGenerator.Next(IdGenerator.EquipCapture), Does.StartWith("EQ-CAP-"));
        });
    }

    [Test]
    public void Next_BookingId_UniquePerDay()
    {
        var id1 = IdGenerator.Next(IdGenerator.Booking);
        var id2 = IdGenerator.Next(IdGenerator.Booking);
        Assert.That(id2, Is.Not.EqualTo(id1));
    }
}
