
using System.Runtime.InteropServices.Swift;

class position
{
    private Int64 x;
    private Int64 y;
    private Int64 z;

    public position(Int64 xpos, Int64 ypos, Int64 zpos)
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

    public double distanceToo(position target)
    {
        Int64[] targetcoords = target.getPos();
        UInt64 xdiff = Convert.ToUInt64(this.x - targetcoords[0]);
        UInt64 ydiff = Convert.ToUInt64(this.y - targetcoords[1]);
        UInt64 zdiff = Convert.ToUInt64(this.z - targetcoords[2]);

        double magnitude = Math.Sqrt(xdiff ^ 2 + ydiff ^ 2 + zdiff ^ 2);
        return magnitude;
    }
}

class dynamicPosition : position, updateAble
{
    Int64 x;
    Int64 y;
    Int64 z;
    Int64 xvel;
    Int64 yvel;
    Int64 zvel;

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