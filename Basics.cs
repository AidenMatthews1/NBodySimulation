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

    public float[3] unitVectorToo(Position target)
    {
        long xDifference = this.x - target.x;
        long yDifference = this.y - target.y;
        long zDifference = this.z - target.z;
        float magnitude = (x ^ 2 + y ^ 2 + z ^ 2) ^ 0.5;
        return new float[3] = [xDifference / magnitude, yDifference / magnitude, zDifference / magnitude];
    }
}

public class dynamicPosition : Position, updateAble
{
    public Int64 xvel { get; }
    public Int64 yvel { get; }
    public Int64 zvel { get; }
    public float magnitudeVel { get{ updateMagnitude(); return magnitudeVel; } }

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
        this.magnitudeVel = (xvel ^ 2 + yvel ^ 2 + zvel ^ 2) ^ 0.5;
    }
}


public abstract class Volume : updateAble
{
    public static Volume Parent { get; }
    public static Position center { get; }
    public static long lowerXBound { get; }
    public static long upperXBound { get; }
    public static long lowerYBound { get; }
    public static long upperYBound { get; }
    public static long lowerZBound { get; }
    public static long upperZBound { get; }
    public Position COM { get; }
    public float mass { get { updateMass(); return mass; } }

    public Volume(Volume parent,Position Center, Position LowerXBound, Position UpperXBound, Position LowerYBound, Position UpperYBound, Position LowerZBound, Position UpperZBound, byte numAxisSplits)
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

        this.center = Center;
        this.COM = center;
        this.Parent = parent;

        lowerXBound = LowerXBound;
        upperXBound = UpperXBound;
        lowerYBound = LowerYBound;
        upperYBound = UpperYBound;
        lowerZBound = LowerZBound;
        upperZBound = UpperZBound;
    }

    public Volume(Volume parent,Position LowerXBound, Position UpperXBound, Position LowerYBound, Position UpperYBound, Position LowerZBound, Position UpperZBound, byte numAxisSplits): this (parent, LowerXBound,UpperXBound,LowerYBound,UpperYBound, LowerZBound, UpperZBound, numAxisSplits)
    {
        long width = math.abs(upperXBound - lowerXBound) + 1;
        long height = math.abs(upperYBound - lowerYBound) + 1;
        long depth = math.abs(upperZBound - lowerZBound) + 1;
        center = new Position(lowerXBound + (width / 2).ToUInt64, lowerYBound + (height / 2).ToUInt64, lowerZBound + (depth / 2).ToUInt64);
    }

    public virtual RVolume getRoot()
    {
        return Parent.getRoot();
    }

    public virtual List<Volume> getAllParents()
    {
        List<Volume> allParents = new List<Volume>(Parent);
        allParents.AddRange(Parent.getAllParents);
        return allParents;
    }

    public virtual bool withinBoundaries(Position target)
    {
        // this is a very verbose and odd way of checking
        // Done this way to possiby create alternative pathways depending on which axis fails the check

        if (target.x >= lowerXBound  & target.x <= upperXBound)
        {
            if (target.y >= lowerYBound & target.y <= upperYBound)
            {
                if (target.z >= lowerZBound & target.z <= upperZBound)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public virtual void updateMajor()
    {
        updateCOM();
        update();
    }

    public virtual int numBodies()
    {
        // Very Inefficient but not likely to be called ofted
        return getContainedBodies.Count();
    }

    protected virtual Position[,,] calculateChildVolumePositions(long BVminMagnitude, long BVmaxMagnitude, byte numAxisSplits, out bool BVolumes)
    {
        // In Volume so dont have to rewrite in RVolume and AVolume even though BVolume will never use this
        // + 1 because boundaries are inclusive and can be shared between volumes
        long width = math.abs(upperXBound - lowerXBound) + 1;
        long height = math.abs(upperYBound - lowerYBound) + 1;
        long depth = math.abs(upperZBound - lowerZBound) + 1;

        float BVMagRatio = BVmaxMagnitude / BVminMagnitude;

        // check that there is enough space to create children
        if (width < numAxisSplits * BVminMagnitude || height < numAxisSplits * BVminMagnitude || depth < numAxisSplits * BVminMagnitude)
        {
            throw new ArgumentException("Volume has been asked to create children without enough space");
        }

        // check if we are making AVolume or BVolume children
        if (width >= (numAxisSplits * BVminMagnitude) * numAxisSplits && height >= (numAxisSplits * BVminMagnitude) * numAxisSplits && depth >= (numAxisSplits * BVminMagnitude) * numAxisSplits)
        {
            BVolumes = false;

            // Ideally all Volumes should be the same size but for many reasons this may not be the case
            // Volumes further from the center should be larger
            // If possible Volumes closer to the center should be default size
            // eg if the numaxissplits = 3 then the inner two volumes will be the same standard size (some multiple of BVMinMag)
            // while the outer will be a non-standard size larger than BVminMag

            // Finding the "level" of volume we are on so we know the size to create the standard size volumes

            // byte xLevel = 1;

            // while (true)
            // {
            //     if (width > BVmaxMagnitude + BVminMagnitude *) > 1)
            // }


        }
        else
        {
            // from here on its ASSUMED that we want to create BVolumes
            // AND there is the correct amount of space to create the correct amount
            BVolumes = true;
            List<long> xBounds = new List<long>();
            List<long> yBounds = new List<long>();
            List<long> zBounds = new List<long>();

            // get direction from root to current Volume to know which side to put the larger child on
            RVolume root = Parent.getRoot();
            float[] unitVecTooRoot = root.center.unitVectorToo(this.center);
            if (unitVecTooRoot[0] > 0) { sbyte xDir = 1; xBounds.Add(this.lowerXBound);}
            else { sbyte xDir = -1; xBounds.Add(this.upperXBound); }
            if (unitVecTooRoot[1] > 0) { sbyte yDir = 1; yBounds.Add(this.lowerYBound); }
            else { sbyte yDir = -1; yBounds.Add(this.upperYBound); }
            if (unitVecTooRoot[2] > 0) { sbyte zDir = 1; zBounds.Add(this.lowerZBound); }
            else { sbyte zDir = -1; zBounds.Add(this.upperZBound); }

            // possible to do it without these seemingly redundant fields 
            // but because of the directionality having them makes the calculations easier
            long remainingMag = width;
            while (true)
            {
                if (remainingMag > BVmaxMagnitude)
                {
                    xBounds.Add(xBounds.Last() + (BVminMagnitude * xDir));
                    remainingMag = -BVminMagnitude;
                }
                if (remainingMag <= BVmaxMagnitude & remainingMag > BVminMagnitude)
                {
                    if (remainingMag - BVminMagnitude >= BVminMagnitude)
                    {
                        xBounds.Add(xBounds.Last() + (BVminMagnitude * xDir));
                        remainingMag = -BVminMagnitude;
                    }
                    else
                    {
                        if (xDir == 1) { xBounds.Add(this.upperXBound); }
                        else { xBounds.Add(this.lowerXBound); }
                    }
                }
                if (remainingMag < BVminMagnitude)
                {
                    throw new ArgumenExeption("Volume calculateChildPositions got itself into an unfixable state (x)");
                }
            }
            









            long xLeftover = width % BVminMagnitude;
            long yLeftover = height % BVminMagnitude;
            long zLeftover = depth % BVminMagnitude;


            if (xLeftover > BVmaxMagnitude - BVminMagnitude | yLeftover > BVmaxMagnitude - BVminMagnitude | zLeftover > BVmaxMagnitude - BVminMagnitude)
            {
                throw new ArgumentException("Volume asked to create BVolumes with not enough slack");
            }

            

            


        }

        







        // if doesnt fit nicely must have volumes of mismatched size
        // if (width % numAxisSplits != 0 || height % numAxisSplits != 0 || depth % numAxisSplits != 0)
        // {


        // }

    }
    public abstract void initialise();    
    public abstract void injestBody(Body newBody);

    public abstract List<Body> getContainedBodies();

    public abstract void update();

    public abstract void updateMass();

    public abstract void updateCOM();

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

    public RVolume(long xlen, long ylen, long zlen, long BVminMagnitude, long BVmaxMagnitude, byte numAxisSplits)
    {
        // length of each axis must be atleast numberVolumeSplits^2 as we need Avolume and BVolume layer below RVolume
        // Length of each axis must be at MOST half the max value of long
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

