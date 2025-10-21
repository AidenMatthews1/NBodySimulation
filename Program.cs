// See https://aka.ms/new-console-template for more information

using Globals;
using Microsoft.Extensions.Logging;

// Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory(
//     new[] {new Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider((_,__) => true, true)}
// );

// Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
// Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.Extensions.Logging.Console.ConsoleLoggerOptions> temp = new Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.Extensions.Logging.Console.ConsoleLoggerOptions>();
// loggerFactory.AddProvider(new Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider(temp));
// Microsoft.Extensions.Logging.ILogger log = loggerFactory.CreateLogger("Console");

// bool logFilter(Microsoft.Extensions.Logging.LogLevel levelCheck) {
//     if levelCheck.
// }

Console.WriteLine($"Starting Program with Log Level: {globalVariables.Level.ToString()}");
Console.WriteLine("Hello, World!");
//Console.WriteLine(globalVariables.units_in_m);

RVolume test = new RVolume(globalVariables.M_in_Lsecond*globalVariables.Units_in_M,8, 8, 8, 2, 2);
Console.WriteLine(test.ToString());



// Basic Class Definitions ----------------------------------------------------

public class positioningException : System.Exception
{
    public string Message { get; } = "Position in spot that should not be possible";
    public Position Offender { get; }
    public positioningException(Position cOffender)
    {
        Offender = cOffender;
    }
    public positioningException(Position cOffender, string cMessage) : this(cOffender)
    {
        Message = cMessage;
    }
    public positioningException(string Message, System.Exception inner) : base(Message, inner) { }
    protected positioningException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class Position
{
    public Int64 x { get; protected set; }
    public Int64 y { get; protected set; }
    public Int64 z { get; protected set; }

    public Position(Int64 xpos, Int64 ypos, Int64 zpos)
    {
        this.x = xpos;
        this.y = ypos;
        this.z = zpos;
    }

    public Position(Position old)
    {
        this.x = old.x;
        this.y = old.y;
        this.z = old.z;
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

    public double[] unitVectorToo(Position target)
    {
       return unitVectorToo(target.x, target.y, target.z);
    }
    
    // TODO Need to think deeply about floating point error here and if I can do anything about it
    public double[] unitVectorToo(long targetx, long targety, long targetz)
    {
        long xDifference = this.x - targetx;
        long yDifference = this.y - targety;
        long zDifference = this.z - targetz;
        double magnitude = Math.Sqrt(xDifference ^ 2 + yDifference ^ 2 + zDifference ^ 2);
        //decimal[] temp = { Convert.ToDecimal(xDifference / magnitude), Convert.ToDecimal(yDifference / magnitude), Convert.ToDecimal(zDifference / magnitude) };
        double[] temp = { Convert.ToDouble(xDifference) / magnitude, Convert.ToDouble(yDifference) / magnitude, Convert.ToDouble(zDifference) / magnitude };
        return temp;
    }

    public override string ToString()
    {
        return $"Position at: {x}, {y}, {x}";
    }
}

public class dynamicPosition : Position, updateAble
{
    public Int64 xvel { get; protected set; }
    public Int64 yvel { get; protected set; }
    public Int64 zvel { get; protected set; }
    //public float magnitudeVel { get { return calcMagnitude(); } protected set; }

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

    // public Int64[] getVel()
    // {
    //     return [this.xvel, this.yvel, this.zvel];
    // }

    protected double calcMagnitude()
    {
        return Math.Sqrt(xvel ^ 2 + yvel ^ 2 + zvel ^ 2);
    }

    public override global::System.String ToString()
    {
        return $"{base.ToString()}. Velocity: {xvel}, {yvel}, {zvel}";
    }
}

public abstract class Volume : updateAble
{
    public virtual Volume Parent { get; protected set; }
    public Position Center { get; protected set; }

    // For the boudaries LowerBound is CLOSEST to RVolume center not most negative
    // In all cases RVolume center can be assumed to be 0 
    public long lowerXBound { get; protected set; }
    public long upperXBound { get; protected set; }
    public long lowerYBound { get; protected set; }
    public long upperYBound { get; protected set; }
    public long lowerZBound { get; protected set; }
    public long upperZBound { get; protected set; }
    public Position COM { get; protected set; }
    public float Mass { get; protected set; }

    protected Volume(Volume cParent, long LowerXBound, long UpperXBound, long LowerYBound, long UpperYBound, long LowerZBound, long UpperZBound)
    {
        this.Mass = 0;
        this.Parent = cParent;

        lowerXBound = LowerXBound;
        upperXBound = UpperXBound;
        lowerYBound = LowerYBound;
        upperYBound = UpperYBound;
        lowerZBound = LowerZBound;
        upperZBound = UpperZBound;

        long width = Math.Abs(upperXBound - lowerXBound) + 1;
        long height = Math.Abs(upperYBound - lowerYBound) + 1;
        long depth = Math.Abs(upperZBound - lowerZBound) + 1;

        this.Center = new Position(lowerXBound + (width / 2), lowerYBound + (height / 2), lowerZBound + (depth / 2));
        this.COM = Center;

        // It is left to the parent to ensure that there are no overlapping Volumes within itself
        // These checks ensure that the volume hasnt been created out of bounds
        if (!Parent.withinBoundaries(Center))
        {
            globalVariables.log.LogError($"Volume is being created with center outside parent bounds\nChild Volume {Center.ToString()}. Parent Volume: {Parent.Center.ToString()}");
            throw new positioningException(Center, "Attempted volume creation outside parent bounds");
        }
    }

    protected Volume() { } // Here because of a Quirk of C# Constructor Inheritance, does nothing except stop a random error

    // protected Volume(Volume cParent, long LowerXBound, long UpperXBound, long LowerYBound, long UpperYBound, long LowerZBound, long UpperZBound, byte numAxisSplits) : this(cParent, LowerXBound, UpperXBound, LowerYBound, UpperYBound, LowerZBound, UpperZBound)
    // {
    //     // TODO create constructor where number of RVolume children is different than default axis splits
    // }

    public override string ToString()
    {
        return $"{this.GetType()} with parent {Parent.GetType()}. Centered at {this.Center.ToString()}, total Mass {Mass}.";
    }

    public virtual RVolume getRoot()
    {
        return Parent.getRoot();
    }

    public virtual List<Volume> getAllParents()
    {
        List<Volume> allParents = new List<Volume>();
        allParents.Add(Parent);
        allParents.AddRange(Parent.getAllParents());
        return allParents;
    }

    public virtual bool withinBoundaries(Position target)
    {
        // this is a very verbose and odd way of checking
        // Done this way to possiby create alternative pathways depending on which axis fails the check

        if (target.x >= lowerXBound & target.x <= upperXBound)
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

    protected virtual List<long>[] calculateChildVolumePositions(long BVMagnitude, byte numAxisSplits, out bool BVolumes)
    {
        // In Volume so dont have to rewrite in RVolume and AVolume even though BVolume will never use this
        // + 1 because boundaries are inclusive and can be shared between volumes
        long width = Math.Abs(upperXBound - lowerXBound);
        long height = Math.Abs(upperYBound - lowerYBound);
        long depth = Math.Abs(upperZBound - lowerZBound);

        List<long> xBounds = new List<long>();
        List<long> yBounds = new List<long>();
        List<long> zBounds = new List<long>();

        xBounds.Add(lowerXBound);
        yBounds.Add(lowerYBound);
        zBounds.Add(lowerZBound);

        // check that there is enough space to create children. Mostly a leftover check from before BVolumes had to all be the same size
        // if (width % (numAxisSplits * BVMagnitude) != 0 | height % (numAxisSplits * BVMagnitude) != 0 | depth % (numAxisSplits * BVMagnitude) != 0)
        // {
        //     throw new ArgumentException("Volume has been asked to create children with inconsistent space");
        // }

        // check if we are making AVolume or BVolume children
        if (width >= (numAxisSplits * BVMagnitude) * numAxisSplits & height >= (numAxisSplits * BVMagnitude) * numAxisSplits & depth >= (numAxisSplits * BVMagnitude) * numAxisSplits)
        {
            BVolumes = false;

            // All volumes are the same size but each axis can be different lengths
            long xDivision = width / numAxisSplits;
            long yDivision = height / numAxisSplits;
            long zDivision = depth / numAxisSplits;

            // starts at 1 and is noninclusive < 
            // because the edges are already put in Boundary lists through other code
            for (byte i = 1; i < numAxisSplits; i++)
            {
                xBounds.Add(xBounds.Last() + xDivision);
                yBounds.Add(yBounds.Last() + yDivision);
                zBounds.Add(zBounds.Last() + zDivision);
            }
        }
        else
        {
            // from here on its ASSUMED that we want to create BVolumes
            // No checks are perfomed to make sure we are creating the "correct" amount of BVolumes
            // this just partitions up the space in a way that fits the magnitude constraints
            BVolumes = true;

            if (width % BVMagnitude != 0 | height % BVMagnitude != 0 | depth % BVMagnitude != 0)
            {
                throw new ArgumentException("Volume asked to create BVolumes with wrong amount of space");
            }

            long remainingMag = width;
            while (remainingMag > 0)
            {
                if (remainingMag < BVMagnitude)
                {
                    throw new ArgumentException("Volume calculateChildPositions got itself into an unfixable state (x)");
                }
                xBounds.Add(xBounds.Last() + BVMagnitude);
                remainingMag = -BVMagnitude;
            }

            remainingMag = height;
            while (remainingMag > 0)
            {
                if (remainingMag < BVMagnitude)
                {
                    throw new ArgumentException("Volume calculateChildPositions got itself into an unfixable state (y)");
                }

                yBounds.Add(yBounds.Last() + BVMagnitude);
                remainingMag = -BVMagnitude;
            }

            remainingMag = depth;
            while (remainingMag > 0)
            {
                if (remainingMag < BVMagnitude)
                {
                    throw new ArgumentException("Volume calculateChildPositions got itself into an unfixable state (z)");
                }

                zBounds.Add(zBounds.Last() + BVMagnitude);
                remainingMag = -BVMagnitude;
            }

        }

        xBounds.Add(upperXBound);
        yBounds.Add(upperYBound);
        zBounds.Add(upperZBound);

        if (xBounds.Count() < 2 | yBounds.Count() < 2 | zBounds.Count() < 2)
        {
            throw new IndexOutOfRangeException($"Calculate Child Volumes couldnt create enough bounds on atleast 1 axis {this.ToString()}");
        }

        List<long>[] temp = { xBounds, yBounds, zBounds };
        globalVariables.log.LogTrace($"CalculateChildVolume sucessfully returned with axis bounds for {temp[0].Count()-1},{temp[1].Count()-1},{temp[2].Count()-1} children. BVolumes:{BVolumes}");
        return temp;
    }

    protected virtual List<Body> getMassiveBodies()
    {
        List<Body> allBodies = getContainedBodies();
        List<Body> massiveBodies = new List<Body>();
        foreach (Body body in allBodies)
        {
            if (body.Massive)
            {
                massiveBodies.Add(body);
            }
        }
        return massiveBodies;
    }

    public abstract void initialise();

    public abstract void injestBody(Body newBody);

    public abstract List<Body> getContainedBodies();

    //public abstract int numBodies();

    public abstract void update();

    //public abstract void updateMass();

    public abstract void updateCOM();
}

public class BVolume : Volume
{
    private Volume[]? simplifiedInteractionVolumes;
    // This is sketchy not sure if I want to set it up like this long term
    //public List<Body>? Children { get { return new List<Body>().AddRange(NMChildren).AddRange(MChildren); } private set; }
    public List<Body> NMChildren { get; private set; }
    public List<Body> MChildren { get; private set; }

    public BVolume(Volume cParent, long LowerXBound, long UpperXBound, long LowerYBound, long UpperYBound, long LowerZBound, long UpperZBound) : base(cParent, LowerXBound, UpperXBound, LowerYBound, UpperYBound, LowerZBound, UpperZBound)
    {
        globalVariables.log.LogTrace($"BVolume constructor called with boundaries  {LowerXBound},  {UpperXBound},  {LowerYBound},  {UpperYBound},  {LowerZBound},  {UpperZBound}");
        MChildren = new List<Body>();
        NMChildren = new List<Body>();

    }
    // public override void initialise()
    // {

    // }
    public override void initialise()
    {
        throw new System.NotImplementedException();
    }

    public override List<Body> getContainedBodies()
    {
        List<Body> Children = new List<Body>();
        Children.AddRange(NMChildren);
        Children.AddRange(MChildren);
        return Children;
    }

    public override void updateCOM()
    {
        // Done with temp values to limit possible race conditions in multithread scenario.
        // If reseting mass and COM while doing calculations any other threads will be getting incorrect data
        // Better to give them out of date values instead
        //Position newCOM = new Position(Center);
        float newMass = 0;
        double xPosOffset = 0;
        double yPosOffset = 0;
        double zPosOffset = 0;

        foreach (Body child in MChildren)
        {
            float Ratio = (newMass + child.Mass) / child.Mass;
            // Calculating this every time instead of storing an updating might be a pretty slow way of doing this 
            double[] Direction = child.Position.unitVectorToo(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
            xPosOffset += Direction[0] * (double)Ratio;
            yPosOffset += Direction[1] * (double)Ratio;
            zPosOffset += Direction[2] * (double)Ratio;
            newMass += child.Mass;
        }
        
        this.COM = new Position(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
        this.Mass = newMass;
    }

    public override void update()
    {
        throw new System.NotImplementedException();
    }


    public override void injestBody(Body newBody)
    {
        if (this.withinBoundaries(newBody.Position))
        {
            if (newBody.Massive)
            {
                MChildren.Add(newBody);
            }
            else
            {
                NMChildren.Add(newBody);
            }
            newBody.Parent = this;
        }
        else
        {
            Parent.injestBody(newBody);
        }
    }

    // public override void updateMass()
    // {
    //     // Done with temp values to limit possible race condition in multithread scenario
    //     float tempMass = 0;
    //     foreach (Body child in MChildren)
    //     {
    //         tempMass += child.Mass;
    //     }
    //     this.Mass = tempMass;
    // }
}

public class RVolume : Volume
{
    // Root Volume is special it should act as the controller for any disjoint areas of the simulation
    public List<AVolume> Children { get; private set; }
    public int Timestep { get; private set; }

    // Accessing the Parent doesnt make sense and its set to itself or null anyway
    // TODO add log entry on get
    override public Volume Parent { get { globalVariables.log.LogWarning("Attempted to get RVolume Parent"); return this; } }

    public RVolume(long BVMagnitude,long xlen, long ylen, long zlen, byte numAxisSplits, byte RVolumeSplits)
    {
        Children = new List<AVolume>();

        bool BVolumes = false;
        this.Center = new Position(0, 0, 0);
        this.COM = Center;
        this.Timestep = 0;
        // length of each axis must be atleast numberVolumeSplits^2 as we need Avolume and BVolume layer below RVolume
        // Length of each axis must be at MOST half the max value of long
        if (xlen*BVMagnitude > Int64.MaxValue / 2 | ylen*BVMagnitude > Int64.MaxValue / 2 | zlen*BVMagnitude > Int64.MaxValue / 2)
        {
            throw new IndexOutOfRangeException("atleast 1 axis of RVolume is too large");
        }

        this.lowerXBound = Convert.ToInt64(BVMagnitude*-Math.Floor(Convert.ToDouble(xlen) / 2));
        this.upperXBound = Convert.ToInt64(BVMagnitude*Math.Ceiling(Convert.ToDouble(xlen) / 2));
        this.lowerYBound = Convert.ToInt64(BVMagnitude*-Math.Floor(Convert.ToDouble(ylen) / 2));
        this.upperYBound = Convert.ToInt64(BVMagnitude*Math.Ceiling(Convert.ToDouble(ylen) / 2));
        this.lowerZBound = Convert.ToInt64(BVMagnitude*-Math.Floor(Convert.ToDouble(zlen) / 2));
        this.upperZBound = Convert.ToInt64(BVMagnitude*Math.Ceiling(Convert.ToDouble(zlen) / 2));
        this.Parent = this;

        globalVariables.log.LogTrace($"RVolume begining child creation with bounds {lowerXBound}, {upperXBound}, {lowerYBound}, {upperYBound}, {lowerZBound}, {upperZBound}");


        List<long>[] boundaries = calculateChildVolumePositions(BVMagnitude, RVolumeSplits, out BVolumes);

        //RVolume assumes its direct children are AVolumes, 
        if (BVolumes)
        {
            throw new ArgumentException("RVolume was created without enough space in atleast 1 axis");
        }

        List<long> xBoundary = boundaries[0];
        List<long> yBoundary = boundaries[1];
        List<long> zBoundary = boundaries[2];
        
        for (int x = 0; x < xBoundary.Count()-1; x++)
        {
            for (int y = 0; y < yBoundary.Count()-1; y++)
            {
                for (int z = 0; z < zBoundary.Count()-1; z++)
                {
                    Children.Add(new AVolume(this, BVMagnitude, xBoundary[x], xBoundary[x + 1], yBoundary[y], yBoundary[y + 1], zBoundary[z], zBoundary[z + 1], numAxisSplits));
                }
            }
        }
    }

    public RVolume(long BVMagnitude, long xlen, long ylen, long zlen, byte numAxisSplits) : this(BVMagnitude, xlen, ylen, zlen, numAxisSplits, numAxisSplits)
    {

    }

    public override global::System.String ToString()
    {
        return $"Root RVolume with {Children.Count()} Direct children containing {getContainedBodies().Count()} bodies, currently on Timestep {Timestep}.";
    }

    public override void initialise()
    {
        foreach(AVolume child in Children)
        {
            child.initialise();
        }
    }

    // public override void updateMass()
    // {
    //     throw new System.NotImplementedException();
    // }

    public override void updateCOM()
    {
        throw new System.NotImplementedException();
    }

    public override void injestBody(Body newBody)
    {
        if (this.withinBoundaries(newBody.Position))
        {
            AVolume targetChild = Children.First();
            double distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);

            foreach (AVolume child in Children)
            {
                if (newBody.Position.distanceToo(child.Center) < distanceTooTarget)
                {
                    targetChild = child;
                    distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);
                }
            }
            targetChild.injestBody(newBody);
        }
        else
        {
            // TODO this should concat body position to be within limits and write to log
            throw new ArgumentException("RVolume was asked to injest a body outside its limits");
        }
    }

    public override void update()
    {
        foreach (Volume child in Children)
        {
            child.update();
        }
        Timestep += 1;
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

    // public override List<BVolume> getBVolumes()
    // {
    //     List<BVolume> allBVolumes = new List<BVolume>();
    //     foreach (AVolume item in Children)
    //     {
    //         allBVolumes.Add(item.getBVolumes());
    //     }
    //     return allBVolumes;
    // }

    public override List<Volume> getAllParents()
    {
        List<Volume> temp = new List<Volume>();
        temp.Add(this);
        return temp;
    }

    public override List<Body> getContainedBodies()
    {
        List<Body> containedBodies = new List<Body>();
        foreach (AVolume child in Children)
        {
            containedBodies.AddRange(child.getContainedBodies());
        }
        return containedBodies;
    }

}

public class AVolume : Volume
{
    public List<Volume> Children { get; private set; }

    public AVolume(Volume cParent, long BVMagnitude, long LowerXBound, long UpperXBound, long LowerYBound, long UpperYBound, long LowerZBound, long UpperZBound, byte numAxisSplits) : base(cParent, LowerXBound, UpperXBound, LowerYBound, UpperYBound, LowerZBound, UpperZBound)
    {
        // Base constructor handles Base Fields this constructor is mostly for creating children

        globalVariables.log.LogTrace($"AVolume constructor called with boundaries {LowerXBound},  {UpperXBound},  {LowerYBound},  {UpperYBound},  {LowerZBound},  {UpperZBound}");
        Children = new List<Volume>();

        bool BVolumes = false;
        List<long>[] boundaries = calculateChildVolumePositions(BVMagnitude, numAxisSplits, out BVolumes);

        List<long> xBoundary = boundaries[0];
        List<long> yBoundary = boundaries[1];
        List<long> zBoundary = boundaries[2];

        // Count starts at 1 but indexing starts at 0. Additionally we dont want to loop through the last boundary
        for (int x = 0; x < xBoundary.Count() - 1; x++)
        {
            for (int y = 0; y < yBoundary.Count() - 1; y++)
            {
                for (int z = 0; z < zBoundary.Count() - 1; z++)
                {
                    if (BVolumes)
                    {
                        Children.Add(new BVolume(this, xBoundary[x], xBoundary[x + 1], yBoundary[y], yBoundary[y + 1], zBoundary[z], zBoundary[z + 1]));

                    }
                    else
                    {
                        Children.Add(new AVolume(this, BVMagnitude, xBoundary[x], xBoundary[x + 1], yBoundary[y], yBoundary[y + 1], zBoundary[z], zBoundary[z + 1], numAxisSplits));
                    }
                }
            }
        }    
    }
    
    // public List<BVolume> getBVolumes()
    // {
    //     return new List<BVolume>();
    // }

    public override void updateCOM()
    {
        // See BVolume updateCom for rational and explanation
        float newMass = 0;
        double xPosOffset = 0;
        double yPosOffset = 0;
        double zPosOffset = 0;

        foreach (Volume child in Children)
        {
            child.updateCOM();
            float Ratio = (newMass + child.Mass) / child.Mass;
            // Calculating this every time instead of storing an updating might be a pretty slow way of doing this 
            double[] Direction = child.COM.unitVectorToo(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
            xPosOffset += Direction[0] * (double)Ratio;
            yPosOffset += Direction[1] * (double)Ratio;
            zPosOffset += Direction[2] * (double)Ratio;
            newMass += child.Mass;
        }
        
        this.COM = new Position(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
        this.Mass = newMass;
    }

    public override void initialise()
    {
        foreach (Volume child in Children){
            child.initialise();
        }
    }

    public override void injestBody(Body newBody)
    {
        if (this.withinBoundaries(newBody.Position))
        {
            Volume targetChild = Children.First();
            double distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);

            foreach (Volume child in Children)
            {
                if (newBody.Position.distanceToo(child.Center) < distanceTooTarget)
                {
                    targetChild = child;
                    distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);
                }
            }
            targetChild.injestBody(newBody);
        }
        else
        {
            Parent.injestBody(newBody);
        }
    }

    public override List<Body> getContainedBodies()
    {
        List<Body> containedBodies = new List<Body>();
        foreach (Volume child in Children)
        {
            containedBodies.AddRange(child.getContainedBodies());
        }
        return containedBodies;
    }

    public override void update()
    {
        foreach (Volume child in Children)
        {
            child.update();
        }
    }
}

public class Body
{
    public dynamicPosition Position { get; set; }

    public ulong Radius { get; protected set; }

    public Volume Parent { get; set; }

    public float Mass { get; protected set; }

    public bool Massive { get; protected set; }

    //public void applyForce()
}
