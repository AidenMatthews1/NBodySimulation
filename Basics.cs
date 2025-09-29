using Globals;

 public class Position
{
    private Int64 x;
    private Int64 y;
    private Int64 z;

    public Position(Int64 xpos, Int64 ypos, Int64 zpos)
    {
        this.x = xpos;
        this.y = ypos;
        this.z = zpos;
    }

    public Int64[] getPos()
    {
        return [this.x, this.y, this.z];
    }

    public void setPos(Int64 xpos, Int64 ypos, Int64 zpos)
    {
        this.x = xpos;
        this.y = ypos;
        this.z = zpos;
    }

    public double distanceToo(Position target)
    {
        Int64[] targetcoords = target.getPos();
        UInt64 xdiff = Convert.ToUInt64(this.x - targetcoords[0]);
        UInt64 ydiff = Convert.ToUInt64(this.y - targetcoords[1]);
        UInt64 zdiff = Convert.ToUInt64(this.z - targetcoords[2]);

        double magnitude = Math.Sqrt(xdiff ^ 2 + ydiff ^ 2 + zdiff ^ 2);
        return magnitude;
    }
}

public class dynamicPosition : Position, updateAble
{
    private Int64 x;
    private Int64 y;
    private Int64 z;
    private Int64 xvel;
    private Int64 yvel;
    private Int64 zvel;

    public dynamicPosition(Int64 xpos, Int64 ypos, Int64 zpos) : base(xpos, ypos, zpos)
    {

        this.xvel = 0;
        this.yvel = 0;
        this.zvel = 0;
    }

    public dynamicPosition(Int64 xpos, Int64 ypos, Int64 zpos, Int64 xveloc, Int64 yveloc, Int64 zveloc) : base(xpos, ypos, zpos)
    {
        this.xvel = xveloc;
        this.yvel = yveloc;
        this.zvel = zveloc;
    }

    public void update()
    {
        this.x += this.xvel;
        this.y += this.yvel;
        this.z += this.zvel;
    }

    public void updateMajor()
    {
        update();
    }

    public void addVel(Int64 xveloc, Int64 yveloc, Int64 zveloc)
    {
        this.xvel += xveloc;
        this.yvel += yveloc;
        this.zvel += zveloc;
    }

    public Int64[] getVel()
    {
        return [this.xvel, this.yvel, this.zvel];
    }
}       


public abstract class Volume : updateAble
{
    public Position Center { get; }
    public Position COM { get; }

    public float mass { get; }

    public Volume Parent { get; }

    



    abstract void initialise();

    abstract void injestBody(Body newBody);

    abstract list<Body> getChildren();


    abstract void update();
    abstract void updateMajor();

}


class Body
{
    protected dynamicPosition pos { get; }

    public ulong radius { get; }

    public Volume parent { get; set; }

    public float mass { get; }

    public bool massive { get; }



}

