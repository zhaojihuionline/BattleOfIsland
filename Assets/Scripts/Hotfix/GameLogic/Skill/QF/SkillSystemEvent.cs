
using cfg;

public class SkillSystemEvent
{

}

public class ReleaseSkillEvent
{
    public SkillPacket packet;
    public ReleaseSkillEvent(SkillPacket table)
    {
        this.packet = table;
    }
}


