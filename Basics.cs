using Globals;

public class positioningException : System.Exception
{
    public string message { get; } = "Position in spot that should not be possible";
    public Position offender { get; }
    public positioningException(Position Offender)
    {
        offender = Offender;
    }
    public positioningException(Position Offender, string Message)
    {
        offender = Offender;
        message = Message;
    }
    public positioningException(string message, System.Exception inner) : base(message, inner) { }
    protected positioningException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class Position
{
    public Int64 x { get; }
    public Int64 y { get; }
    public Int64 z { get; }

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
    public Int64 xvel { get; }
    public Int64 yvel { get; }
    public Int64 zvel { get; }
    public float magnitude { get; }

    public dynamicPosition(Int64 xpos, Int64 ypos, Int64 zpos) : base(xpos, ypos, zpos)
    {

        this.xvel = 0;
        this.yvel = 0;
        this.zvel = 0;
        this.magnitude = 0;
    }

    public dynamicPosition(Int64 xpos, Int64 ypos, Int64 zpos, Int64 xveloc, Int64 yveloc, Int64 zveloc) : base(xpos, ypos, zpos)
    {
        this.xvel = xveloc;
        this.yvel = yveloc;
        this.zvel = zveloc;
        updateMagnitude();
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

    // public Int64[] getVel()
    // {
    //     return [this.xvel, this.yvel, this.zvel];
    // }

    protected void updateMagnitude()
    {
        this.magnitude = (xvel ^ 2 + yvel ^ 2 + zvel ^ 2) ^ 0.5;
    }
}


public abstract class Volume : updateAble
{
    public Position Center { get; }
    public Position COM { get; }

    public float mass { get { updateMass(); return mass; } }

    public Volume Parent { get; }
    public UInt64 xlen { get; }
    public UInt64 ylen { get; }
    public UInt64 zlen { get; }

    public Volume(Position center, Volume parent, long xlen, long ylen, long zlen)
    {
        // It is left to the parent to ensure that there are no overlapping Volumes within itself
        // These checks ensure that the volume hasnt been created out of bounds
        if (!parent.withinBoundaries(center))
        {
            throw new positioningException(center, "Attempted volume creation outside parent bounds");
        }
        if (!parent.withinBoundaries(new Position(center.x + xlen, center.y + ylen, center.z + zlen)) | !parent.withinBoundaries(new Position(center.x - xlen, center.y - ylen, center.z - zlen)))
        {
            throw new positioningException(center, "Attempted volume creation with bounds outside parent");
        }

        this.mass = 0;
        this.Center = center;

        this.COM = center;
        this.Parent = parent;

    }

    public virtual RVolume getRoot()
    {
        return Parent.getRoot();
    }

    public abstract void initialise();

    public virtual List<Volume> getAllParents()
    {
        List<Volume> allParents = new List<Volume>(Parent);
        allParents.AddRange(Parent.getAllParents);
        return allParents;    
    } 

    public abstract void injestBody(Body newBody);

    public abstract List<Body> getChildren();

    public virtual bool withinBoundaries(Position target)
    {
        // this is a very verbose and odd way of checking
        // Done this way to possiby create alternative pathways depending on which axis fails the check

        if (target.x >= this.Center.x + this.xlen & target.x <= this.Center.x - this.xlen)
        {
            if (target.y >= this.Center.y + this.ylen & target.y <= this.Center.y - this.ylen)
            {
                if (target.z >= this.Center.z + this.zlen & target.z <= this.Center.z - this.zlen)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public abstract void update();

    public abstract void updateMajor();

    public abstract void updateMass();

    public abstract int numBodies();
}

public class BVolume : Volume
{
    private Volume[] simplifiedInteractionVolumes;
    public List<Body> Children { get; }


    public BVolume(Position center, Volume parent, long xlen, long ylen, long zlen) : base(center, parent, xlen, ylen, zlen)
    {
        Children = new List<Body>();
        simplifiedInteractionVolumes = new Volume[0];
    }
    // public override void initialise()
    // {

    // }
}
public class RVolume : Volume
{
    // Root Volume is special. Its parent is itself and shouldnt ever be used
    public List<AVolume> Children { get; }
    public int Timestep { get; }

    // Accessing the Parent doesnt make sense and its set to itself or null anyway
    public Volume? Parent { get { throw new InvalidOperationException("Tried to access RVolume Parent"); } }

    public RVolume(long xlen, long ylen, long zlen, long BVminMagnitude, long BVmaxMagnitude, byte numVolumeSplitsperAxis)
    {
        // length must be atleast numberVolumeSplits^2 as we need Avolume and BVolume layer below RVolume
    }
    public override void update()
    {
        foreach (Volume child in directChildren)
        {
            child.update();
        }
    }

    public void updateMany(int numUpdates)
    {
        for (int temp = 0; temp < numUpdates; temp += 1)
        {
            update();
        }
    }

    public override RVolume getRoot()
    {
        return this;
    }

    public override List<BVolume> getBVolumes()
    {
        List<BVolume> allBVolumes = new List<BVolume>();
        foreach (AVolume item in Children)
        {
            allBVolumes.Add(item.getBVolumes());
        }
        return allBVolumes;
    }

    public override List<Volume> getAllParents()
    {
        return new List<Volume>(this);
    }
}

public class AVolume : Volume {
    private List<Volume> Children;


    public List<BVolume> getBVolumes()
    {
        return new List<BVolume>();
    }
}
public class Body
{
    protected dynamicPosition pos { get; }

    public ulong radius { get; }

    public Volume parent { get; set; }

    public float mass { get; }

    public bool massive { get; }
}

