// See https://aka.ms/new-console-template for more information

using Globals;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

Console.WriteLine($"Starting Program with Log Level: {globalVariables.Level.ToString()}");

//if (!ThreadPool.SetMaxThreads(1, 1)) Console.WriteLine("Threadpool not listening");

//public static WaitHandle[] waitHandles = new WaitHandle[numThreads];



RVolume root = new RVolume(1000000 * globalVariables.Units_in_M, 4, 4, 4, 2, 2);
// Body body1 = new Body((double)Math.Pow(10,15), 190000, 0, 0);
// Body body2 = new Body(1, 0, 0, 0);
// test.injestBody(body1);
// test.injestBody(body2);

IEnumerable<string> rows = File.ReadLines("bodies.csv");

foreach (string row in rows)
{
    string[] values = row.Split(',');
    if (values.Length != 8)
    {
        throw new ArgumentException($"CSV Row was the wrong size {values.Length}");
    }
    else
    {
        root.injestBody(new Body(Guid.Parse(values[0]), Convert.ToDouble(values[1]), long.Parse(values[2]), long.Parse(values[3]), long.Parse(values[4]), long.Parse(values[5]), long.Parse(values[6]), long.Parse(values[7])));
    }
}
globalVariables.log.LogInformation($"Successfully ingested {root.getContainedBodies().Count()} Bodies");


root.initialise();
globalVariables.log.LogInformation(root.ToString());
// Console.WriteLine(body1.ToString());
// Console.WriteLine(body2.ToString());

var watch = Stopwatch.StartNew();
root.updateMany(1000);
watch.Stop();
Console.WriteLine("Claimed time:");
Console.WriteLine(watch.ElapsedMilliseconds.ToString());

// while(Console.ReadLine() != "s")
// {
//     Console.WriteLine("NEW UPDATE");
//     foreach (Body a in test.getContainedBodies())
//     {
//         Console.WriteLine(a.ToString());
//     }
//     Console.WriteLine("--------------");
//     test.update();
// }

// Basic Class Definitions ----------------------------------------------------

public class positioningException : System.Exception
{
    public string Message { get; } = "Position in spot that should not be possible";
    public Vector Offender { get; }
    public positioningException(Vector cOffender)
    {
        Offender = cOffender;
    }
    public positioningException(Vector cOffender, string cMessage) : this(cOffender)
    {
        Message = cMessage;
    }
    public positioningException(string Message, System.Exception inner) : base(Message, inner) { }
    protected positioningException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class Vector
{
    //TODO a better design would have these sets protected
    public Int64 x { get; set; }
    public Int64 y { get; set; }
    public Int64 z { get; set; }

    public Vector(Int64 xpos, Int64 ypos, Int64 zpos)
    {
        this.x = xpos;
        this.y = ypos;
        this.z = zpos;
    }

    public Vector(Vector old)
    {
        this.x = old.x;
        this.y = old.y;
        this.z = old.z;
    }

    public Int64[] getPos()
    {
        return [this.x, this.y, this.z];
    }

    //public static implicit operator long[](Vector a) => new long[] {a.x, a.y, a.z}; 

    public decimal distanceToo(Vector target)
    {
        long xdiff = this.x - target.x;
        long ydiff = this.y - target.y;
        long zdiff = this.z - target.z;

        decimal magnitude = (decimal)Math.Sqrt(Math.Pow(xdiff, 2) + Math.Pow(ydiff, 2) + Math.Pow(zdiff, 2));
        return magnitude;
    }

    public decimal[] unitVectorToo(Vector target)
    {
        return unitVectorToo(target.x, target.y, target.z);
    }

    // TODO Need to think deeply about floating point error here and if I can do anything about it
    public decimal[] unitVectorToo(long targetx, long targety, long targetz)
    {
        long xDifference = targetx - this.x;
        long yDifference = targety - this.y;
        long zDifference = targetz - this.z;

        // Console.WriteLine(xDifference.ToString());
        // Console.WriteLine(yDifference.ToString());
        // Console.WriteLine(zDifference.ToString());

        decimal magnitude = (decimal)(Math.Sqrt(Math.Pow(xDifference, 2) + Math.Pow(yDifference, 2) + Math.Pow(zDifference, 2)));
        //decimal[] temp = { Convert.ToDecimal(xDifference / magnitude), Convert.ToDecimal(yDifference / magnitude), Convert.ToDecimal(zDifference / magnitude) };
        decimal[] temp = { Convert.ToDecimal(xDifference) / magnitude, Convert.ToDecimal(yDifference) / magnitude, Convert.ToDecimal(zDifference) / magnitude };
        return temp;
    }

    public Vector vectorToo(Vector target)
    {
        return vectorToo(target.x, target.y, target.z);
    }

    public Vector vectorToo(long targetx, long targety, long targetz)
    {
        long xDifference = targetx - this.x;
        long yDifference = targety - this.y;
        long zDifference = targetz - this.z;
        return new Vector(xDifference, yDifference, zDifference);
    }

    public decimal magnitude()
    {
        return checked((decimal)(Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2) + Math.Pow(z,2))));
    }

    public override string ToString()
    {
        return $"{x}, {y}, {z}";
    }
}

public class dynamicPosition : Vector, updateAble
{
    // TODO a better design here would have protected sets (Easy to do by changing RVolume injest from direct edit to setVel())
    public Int64 xvel { get; set; }
    public Int64 yvel { get; set; }
    public Int64 zvel { get; set; }
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

    public void update(int timestep)
    {
        this.update();
    }

    public void updateMajor()
    {
        update();
    }

    public void updateMajor(int timestep)
    {
        update();
    }

    public void addVel(Int64 xveloc, Int64 yveloc, Int64 zveloc)
    {
        this.xvel += xveloc;
        this.yvel += yveloc;
        this.zvel += zveloc;
    }

    public void setPos(Int64 xpos, Int64 ypos, Int64 zpos)
    {
        this.x = xpos;
        this.y = ypos;
        this.z = zpos;
    }

    // public Int64[] getVel()
    // {
    //     return [this.xvel, this.yvel, this.zvel];
    // }

    protected decimal velMagnitude()
    {
        return (decimal)(Math.Sqrt(Math.Pow(xvel,2) + Math.Pow(yvel,2) + Math.Pow(zvel,2)));
    }

    public override global::System.String ToString()
    {
        return $"{base.ToString()}. Velocity: {xvel}, {yvel}, {zvel}";
    }
}

public abstract class Volume : updateAble, mass
{
    public virtual Volume Parent { get; protected set; }
    public Vector Center { get; protected set; }

    // For the boudaries LowerBound is CLOSEST to RVolume center not most negative
    // In all cases RVolume center can be assumed to be 0 
    public long lowerXBound { get; protected set; }
    public long upperXBound { get; protected set; }
    public long lowerYBound { get; protected set; }
    public long upperYBound { get; protected set; }
    public long lowerZBound { get; protected set; }
    public long upperZBound { get; protected set; }
    public Vector COM { get; protected set; }
    public double Mass { get; protected set; }

    public Guid id { get; protected set; }
    public int Timestep { get; protected set; }


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

        this.Center = new Vector(lowerXBound + (width / 2), lowerYBound + (height / 2), lowerZBound + (depth / 2));
        this.COM = Center;

        // It is left to the parent to ensure that there are no overlapping Volumes within itself
        // These checks ensure that the volume hasnt been created out of bounds
        if (!Parent.withinBoundaries(Center))
        {
            globalVariables.log.LogError($"Volume is being created with center outside parent bounds\nChild Volume {Center.ToString()}. Parent Volume: {Parent.Center.ToString()}");
            throw new positioningException(Center, "Attempted volume creation outside parent bounds");
        }

        this.id = Guid.NewGuid();
    }

    protected Volume() { } // Here because of a Quirk of C# Constructor Inheritance, does nothing except stop a random error

    // protected Volume(Volume cParent, long LowerXBound, long UpperXBound, long LowerYBound, long UpperYBound, long LowerZBound, long UpperZBound, byte numAxisSplits) : this(cParent, LowerXBound, UpperXBound, LowerYBound, UpperYBound, LowerZBound, UpperZBound)
    // {
    //     // TODO create constructor where number of RVolume children is different than default axis splits
    // }

    public override string ToString()
    {
        return $"{this.GetType()} {this.id.ToString()} Centered at {this.Center.ToString()}, total Mass {Mass} with parent {Parent.GetType()}.";
    }

    public string idToString()
    {
        return $"{this.GetType().ToString()} {this.id.ToString()}";
    }

// will spin until the number of threads being used <= input int
    public void activeThreadWaiter(int numActiveThreads)
    {
        while (true)
        {
            int placeholder = 0;
            int maxThreads = 0;
            int availThreads = 0;
            ThreadPool.GetMaxThreads(out maxThreads, out placeholder);
            ThreadPool.GetAvailableThreads(out availThreads, out placeholder);
            //Console.WriteLine((maxThreads - availThreads).ToString());

            //Console.WriteLine(maxThreads.ToString());
            //Console.WriteLine(availThreads.ToString());
            if (maxThreads - availThreads <= numActiveThreads)
            {
                return;
            }
        }
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

    public virtual bool withinBoundaries(Vector target)
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
    public abstract void injestBody(Body newBody, int timestep);

    public abstract List<Body> getContainedBodies();

    //public abstract int numBodies();

    public abstract void update();

    public abstract void updateCleanup();

    public abstract void updateCOM();
}

public class BVolume : Volume
{
    private Volume[]? simplifiedInteractionVolumes;
    private BVolume[]? fullInteractionVolumes;
    // This is sketchy not sure if I want to set it up like this long term
    //public List<Body>? Children { get { return new List<Body>().AddRange(NMChildren).AddRange(MChildren); } private set; }
    public List<Body> NMChildren { get; private set; }

    // This should have protected set and a method to provide a readonly list but I cant figure it out and I dont have time ot figure it out
    public List<Body> MChildren { get; set; }

    // there needs to be a better way of doing this
    private List<Body> allChildren { get { List<Body> temp = new List<Body>(); temp.AddRange(MChildren); temp.AddRange(NMChildren); return temp; } }

    public List<Body> futureChildren { get; protected set; }

    public int lastTimestep;

    public BVolume(Volume cParent, long LowerXBound, long UpperXBound, long LowerYBound, long UpperYBound, long LowerZBound, long UpperZBound) : base(cParent, LowerXBound, UpperXBound, LowerYBound, UpperYBound, LowerZBound, UpperZBound)
    {
        //globalVariables.log.LogTrace($"BVolume constructor called with boundaries  {LowerXBound},  {UpperXBound},  {LowerYBound},  {UpperYBound},  {LowerZBound},  {UpperZBound}");
        MChildren = new List<Body>();
        NMChildren = new List<Body>();
        futureChildren = new List<Body>();
        globalVariables.log.LogTrace($"BVolume constructed: {this.ToString()}");
        lastTimestep = 0;
    }

    public override void initialise()
    {
        List<Volume> candidates = new List<Volume>();
        candidates.AddRange(getRoot().Children);
        // TODO these simplified Interactions effects are the same for every body can do some simplifications here
        List<Volume> simplifiedInteractions = new List<Volume>();
        List<BVolume> fullInteractions = new List<BVolume>();

        decimal calcAngle(Vector a, Vector b)
        {
            // theta = cos^(-1)(a.b/(|a||b|))
            // product of the magnitudes is likely to result in an overflow 
            // Dot product can be as large as the product of the magnitudes of the vectors as well so it could also result in an overflow even using longs

            // To make it slightly less likely to overflow I will separate the dot product calculation into individual axis, and separate the divisions 
            // Each axis of the dot product will be divided by |a| and then the sum will be divided by |b|
            // This way the entire dot product and the product of the magnitde doesnt need to be calculated. Still dealing with very large numbers but not as large as product of magnitudes  
            // Thought about individually dividing the Axis BEFORE multiplication but that will result in very small deximals instead which is a whole other can of worms

            decimal angle;
            decimal temp;
            try
            {
                temp = checked(((Convert.ToDecimal(a.x) * b.x) / a.magnitude() + (Convert.ToDecimal(a.y) * b.y) / a.magnitude() + (Convert.ToDecimal(a.z) * b.z) / a.magnitude()) / b.magnitude());
                //angle = (decimal)Math.Acos(checked(((Convert.ToDouble(a.x) /a.magnitude()) * b.x/ a.magnitude() + (Convert.ToDouble(a.y)/a.magnitude()) * b.y/ a.magnitude() + (Convert.ToDouble(a.z)/a.magnitude()) * b.z/ a.magnitude()) / b.magnitude()));
            }
            catch (Exception e)
            {
                globalVariables.log.LogWarning($"An Exception likely Arithmetic Overflow  was silently ignored while initialising. Attempted to calculate angle between two vectors:\n{a.ToString()}\n{b.ToString()}\nmax integer value: {Int64.MaxValue}");
                globalVariables.log.LogTrace((((decimal)(a.x) * b.x) / a.magnitude()).ToString());
                globalVariables.log.LogTrace((((decimal)(a.y) * b.y) / a.magnitude()).ToString());
                globalVariables.log.LogTrace((((decimal)(a.z) * b.z) / a.magnitude()).ToString());
                globalVariables.log.LogTrace(b.magnitude().ToString());
                globalVariables.log.LogWarning(e.Message);

                temp = ((Convert.ToDecimal(a.x) * b.x) / a.magnitude() + (Convert.ToDecimal(a.y) * b.y) / a.magnitude() + (Convert.ToDecimal(a.z) * b.z) / a.magnitude()) / b.magnitude();
                Console.WriteLine(temp);
                
            }

            if (temp > 1)
            {
                angle = 0M;
                globalVariables.log.LogTrace("Angle between vectors was outside bounds likely due to rounding error, setting to 0");
            }
            else
            {
                if (temp < -1)
                {
                    angle = (decimal)Math.PI;
                    globalVariables.log.LogTrace("Angle between vectors was outside bounds likely due to rounding error, setting to 180");
                }
                else
                {
                    try
                    {
                        angle = (decimal)Math.Acos((double)temp);
                    }
                    catch
                    {
                        globalVariables.log.LogWarning("Unable to calculate angle between vectors setting to 0");
                        globalVariables.log.LogWarning(temp.ToString());
                        globalVariables.log.LogWarning(Math.Acos((double)temp).ToString());
                        angle = 0M;
                    }        
                }    
            }
            
            return angle;
        }

        while (candidates.Count() > 0)
        {
            Volume currentTarget = candidates[0];
            if (simplifiedInteractions.Contains(currentTarget) | fullInteractions.Contains(currentTarget))
            {
                globalVariables.log.LogTrace("Removed target already checked");
                globalVariables.log.LogTrace(currentTarget.ToString());
                candidates.RemoveAt(0);
                globalVariables.log.LogTrace(candidates[0].ToString());
                continue;
            }

            if (currentTarget == this)
            {
                //globalVariables.log.LogTrace("Removed self from candidates");
                candidates.RemoveAt(0);
                continue;
            }

            // If one of the parents is already simplified then we dont have to check this candidate
            // This should not be possible but it seems to be happening somewhere
            List<Volume> targetParents = currentTarget.getAllParents();
            foreach (Volume parent in targetParents)
            {
                if (simplifiedInteractions.Contains(parent))
                {
                    globalVariables.log.LogTrace("Removed target with simplified parent");
                    candidates.RemoveAt(0);
                    continue;
                }
            }

            if (currentTarget is RVolume) // legacy but might still be a useful check
            {
                RVolume tempcurrentTarget = currentTarget as RVolume;
                candidates.AddRange(tempcurrentTarget.Children);
                candidates.RemoveAt(0);
                continue;
            }


            Vector lowerTarget = new Vector(Center.x - currentTarget.lowerXBound, Center.y - currentTarget.lowerYBound, Center.z - currentTarget.lowerZBound);
            Vector upperTarget = new Vector(Center.x - currentTarget.upperXBound, Center.y - currentTarget.upperYBound, Center.z - currentTarget.upperZBound);
            decimal angle = calcAngle(lowerTarget, upperTarget);

            // If its far enough away that we can simplify the interactions
            if (angle <= globalVariables.max_angle_initialisation)
            {
                simplifiedInteractions.Add(currentTarget);
                //globalVariables.log.LogTrace($"{this.ToString()} has found simplified interaction {currentTarget.ToString()}");
            }
            else  // Cant simplify the interactions with this volume 
            {
                if (currentTarget is BVolume)
                {
                    fullInteractions.Add(currentTarget as BVolume);
                }
                else
                {
                    if (currentTarget is AVolume)
                    {
                        AVolume tempcurrentTarget = currentTarget as AVolume;
                        candidates.AddRange(tempcurrentTarget.Children);
                    }
                }
                // this if chain will ensure that if somehow an objects not BVolume AVolume or RVolume then it simply gets removed from the list
            }
            candidates.RemoveAt(0);
        }

        this.simplifiedInteractionVolumes = simplifiedInteractions.ToArray();
        this.fullInteractionVolumes = fullInteractions.ToArray();

        this.updateCOM();
        // globalVariables.log.LogTrace($"{this.ToString()} has finished finding simplified interactions {simplifiedInteractions.Count()}, {fullInteractions.Count()}");
        // if(this.Center.x != this.COM.x)
        // {
        //     Console.WriteLine("SUCCESS");
        //     Console.WriteLine(this.COM.ToString());
        // }
    }

    public override List<Body> getContainedBodies()
    {
        List<Body> Children = new List<Body>();
        Children.AddRange(NMChildren);
        Children.AddRange(MChildren);
        return Children;
    }


    // Separated into own function as there may be other special objects in the future and needs to return a read only list somehow
    // public ReadOnlyCollection<mass> getMassiveObjects()
    // {
    //     ReadOnlyCollection<mass> massiveObjects = MChildren;
    //     foreach (mass test in massiveObjects)
    //     {
    //         Console.WriteLine(test);
    //     }
    //     return massiveObjects;
    // } 

    public override void updateCOM()
    {
        // Done with temp values to limit possible race conditions in multithread scenario.
        // If reseting mass and COM while doing calculations any other threads will be getting incorrect data
        // Better to give them out of date values instead
        //Vector newCOM = new Vector(Center);
        double newMass = 0;

        decimal xPosOffset = 0;
        decimal yPosOffset = 0;
        decimal zPosOffset = 0;

        foreach (Body child in MChildren)
        {
            decimal Ratio = (decimal)((newMass + child.Mass) / child.Mass);

            Vector Direction = child.Position.vectorToo(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
            // -= because the direction is pointing the wrong way as its Child to COM not COM to child
            xPosOffset -= Direction.x * Ratio;
            yPosOffset -= Direction.y * Ratio;
            zPosOffset -= Direction.z * Ratio;
            newMass += child.Mass;
        }
        Vector newCOM = new Vector(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
        if (this.withinBoundaries(newCOM))
        {
            this.COM = newCOM;
            this.Mass = newMass;
        }
        else
        {
            globalVariables.log.LogError($"Updating COM resulted in COM outside of boundaries. newCOM has been ignored, falling back to old value. \n{this.ToString()} \n{newCOM.ToString()}");
        }

    }

    public override void update()
    {
        int timestep = Parent.Timestep;
        // Find mass array
        // TODO Would be good to allocate array size at the start instead of dynamically resizing for every new list but that would add complexity we dont need in MVP
        // TODO can likely save simplified interactions as flat veloc applications that get updated on a Major Instead of getting every body to calculate them every time
        List<mass> Influences = new List<mass>();
        Influences.AddRange(MChildren);
        Influences.AddRange(simplifiedInteractionVolumes);
        foreach (BVolume fullInteraction in fullInteractionVolumes)
        {
            Influences.AddRange(fullInteraction.MChildren);
        }

        mass[] influencesArray = Influences.ToArray();

        foreach (Body body in allChildren)
        {
            body.calculateManyForce(influencesArray);
        }

        foreach (Body body in allChildren)
        {
            // applies the velocity to the position
            body.update();
            if (!this.withinBoundaries(body.COM))
            {
                //globalVariables.log.LogTrace($"{this.idToString()} giving up {body.ToString()} as outside boundaries");
                this.Parent.injestBody(body, timestep);
                if (body.Massive)
                {
                    this.MChildren.Remove(body);
                }
                else
                {
                    this.NMChildren.Remove(body);
                }
            }
        }
    }


    public override void injestBody(Body newBody, int timestep)
    {
        if (this.withinBoundaries(newBody.Position))
        {
            globalVariables.log.LogTrace($"{this.idToString()} Ingesting {newBody.idToString()}");
            if (timestep > lastTimestep)
            {
                this.injestBody(newBody);
            }
            else
            {
                futureChildren.Add(newBody);   
            }
        }
        else
        {
            Parent.injestBody(newBody, timestep);
        }
    }

    public override void injestBody(Body newBody)
    {
        if (this.withinBoundaries(newBody.Position))
        {
            globalVariables.log.LogTrace($"{this.idToString()} Ingesting {newBody.idToString()}");
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
    public override void updateCleanup()
    {
        foreach  (Body body in futureChildren)
        {
            this.injestBody(body);
        }
    }

}

public class RVolume : Volume
{
    // Root Volume is special it should act as the controller for any disjoint areas of the simulation
    public List<AVolume> Children { get; private set; }
    public int Timestep { get; private set; }

    override public Volume Parent { get { globalVariables.log.LogWarning("Attempted to get RVolume Parent"); return this; } }

    public RVolume(long BVMagnitude,long xlen, long ylen, long zlen, byte numAxisSplits, byte RVolumeSplits)
    {
        Children = new List<AVolume>();

        bool BVolumes = false;
        this.Center = new Vector(0, 0, 0);
        this.COM = Center;
        this.Timestep = 0;
        // length of each axis must be atleast numberVolumeSplits*RVolumeSplits as we need Avolume and BVolume layer below RVolume
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

        globalVariables.log.LogTrace($"RVolume begining child creation with bounds {lowerXBound}, {lowerYBound}, {lowerZBound}, {upperXBound}, {upperYBound}, {upperZBound}");


        List<long>[] boundaries = calculateChildVolumePositions(BVMagnitude, RVolumeSplits, out BVolumes);

        //RVolume assumes its direct children are AVolumes, 
        if (BVolumes)
        {
            throw new ArgumentException("RVolume was created without enough space in atleast 1 axis");
        }

        List<long> xBoundary = boundaries[0];
        List<long> yBoundary = boundaries[1];
        List<long> zBoundary = boundaries[2];

        for (int x = 0; x < xBoundary.Count() - 1; x++)
        {
            for (int y = 0; y < yBoundary.Count() - 1; y++)
            {
                for (int z = 0; z < zBoundary.Count() - 1; z++)
                {
                    Children.Add(new AVolume(this, BVMagnitude, xBoundary[x], xBoundary[x + 1], yBoundary[y], yBoundary[y + 1], zBoundary[z], zBoundary[z + 1], numAxisSplits));
                }
            }
        }
        
        foreach(AVolume child in Children)
        {
            child.initialise();
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
        if (!this.withinBoundaries(newBody.Position))
        {
             // One or more of the Bodies position values are outside the boundaries. Will edit the relevant ones to be within boundaries
            globalVariables.log.LogTrace("RVolume was asked to injest a body outside its limits");

            if (newBody.Position.x > this.upperXBound)
            {
                newBody.Position.x = upperXBound - 1;
                newBody.Position.xvel -= 1;
            }
            else
            {
                if (newBody.Position.x < this.lowerXBound)
                {
                    newBody.Position.x = lowerXBound + 1;
                    newBody.Position.xvel += 1;
                }
            }
            if (newBody.Position.y > this.upperYBound)
            {
                newBody.Position.y = upperYBound - 1;
                newBody.Position.yvel -= 1;
            }
            else
            {
                if (newBody.Position.y < this.lowerYBound)
                {
                    newBody.Position.y = lowerYBound + 1;
                    newBody.Position.yvel += 1;
                }
            }
            if (newBody.Position.z > this.upperZBound)
            {
                newBody.Position.z = upperZBound - 1;
                newBody.Position.zvel -= 1;
            }
            else
            {
                if (newBody.Position.z < this.lowerZBound)
                {
                    newBody.Position.z = lowerZBound + 1;
                    newBody.Position.zvel += 1;
                }
            }
        }

        AVolume targetChild = Children.First();
        decimal distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);

        foreach (AVolume child in Children)
        {
            if (newBody.Position.distanceToo(child.Center) < distanceTooTarget)
            {
                targetChild = child;
                distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);
            }
        }
        targetChild.injestBody(newBody, Timestep);

    }

    public override void injestBody(Body newBody, int timestep)
    {
        injestBody(newBody);
    }

    public override void update()
    {

        foreach (AVolume child in Children)
        {
            //ThreadPool.QueueUserWorkItem(new WaitCallback(child.update()));
            child.threadedUpdate();
        }
        // wait for all threads to complete
        activeThreadWaiter(0);
        Timestep += 1;
    }

    public override void updateCleanup()
    {
        foreach (AVolume child in Children)
        {
            child.updateCleanup();
        }
    }

    // public void update()
    // {
    //     foreach (Volume child in Children)
    //     {
    //         child.update(Timestep);
    //     }
    //     Timestep += 1;
    // }


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

    public int Timestep {get { return Parent.Timestep; }}

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

    public override void updateCleanup()
    {
        foreach (Volume child in Children)
        {
            child.updateCleanup();
        }
    }
    
    // public List<BVolume> getBVolumes()
    // {
    //     return new List<BVolume>();
    // }

    public override void updateCOM()
    {
        // See BVolume updateCom for rational and explanation
        double newMass = 0;
        decimal xPosOffset = 0;
        decimal yPosOffset = 0;
        decimal zPosOffset = 0;

        foreach (Volume child in Children)
        {
            child.updateCOM();
            decimal Ratio = (decimal)((newMass + child.Mass) / child.Mass);
            // Calculating this every time instead of storing an updating might be a pretty slow way of doing this 
            decimal[] Direction = child.COM.unitVectorToo(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
            xPosOffset += Direction[0] * Ratio;
            yPosOffset += Direction[1] * Ratio;
            zPosOffset += Direction[2] * Ratio;
            newMass += child.Mass;
        }
        
        this.COM = new Vector(this.Center.x + (long)Math.Round(xPosOffset), this.Center.y + (long)Math.Round(yPosOffset), this.Center.z + (long)Math.Round(zPosOffset));
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
            decimal distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);

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

    public override void injestBody(Body newBody, int timestep)
    {
        if (this.withinBoundaries(newBody.Position))
        {
            Volume targetChild = Children.First();
            decimal distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);

            foreach (Volume child in Children)
            {
                if (newBody.Position.distanceToo(child.Center) < distanceTooTarget)
                {
                    targetChild = child;
                    distanceTooTarget = newBody.Position.distanceToo(targetChild.Center);
                }
            }
            targetChild.injestBody(newBody, timestep);
        }
        else
        {
            Parent.injestBody(newBody, timestep);
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

    public void update(object state)
    {
        update();
    }

    public void threadedUpdate()
    {
        activeThreadWaiter(globalVariables.threads - 1);
        ThreadPool.QueueUserWorkItem(new WaitCallback(update), null);
    }
}

public class Body : mass
{
    public Guid id {get; protected set;}
    // Center of Mass used even though not all bodies will be "massive" to comply with mass interface standards
    public dynamicPosition Position { get; set; }

    public Vector COM {get{ return Position as Vector; }}

    public ulong Radius { get; protected set; }

    public Volume? Parent { get; set; }

    public double Mass { get; protected set; }

    public bool Massive { get; protected set; }

    protected Body(double mass)
    {
        
        this.Mass = mass;
        Radius = 1;

        if (mass >= globalVariables.min_massive_weight)
        {
            Massive = true;
        }
        else
        {
            Massive = false;
        }

        if (this.id == null)
        {
            this.id = Guid.NewGuid();
        }        
    }

    public Body(double mass, long x, long y, long z) : this(mass)
    {
        Position = new dynamicPosition(x, y, z);
    }

    public Body(double mass, long x, long y, long z, long xvel, long yvel, long zvel) : this(mass)
    {
        Position = new dynamicPosition(x, y, z, xvel, yvel, zvel);
    }
    
    public Body(Guid id, double mass, long x, long y, long z, long xvel, long yvel, long zvel) : this(mass, x, y, z, xvel, yvel, zvel)
    {
        this.id = id;
    }

    public override string ToString()
    {
        if (this.Parent != null)
        {
            return $"{this.id} Body child of {this.Parent.idToString()} at {this.Position}, {this.Mass}, {this.Massive}, {this.Radius}";
        }
        return $"{this.id} Body at {this.Position}, {this.Mass}, {this.Massive}, {this.Radius}";
    }

    public string idToString()
    {
        return $"Body {this.id}";
    }

    public void applyForceNewtons(double forcex, double forcey, double forcez)
    {
        long velx = (long)(forcex / Mass);
        long vely = (long)(forcey / Mass);
        long velz = (long)(forcez / Mass);

        this.Position.addVel(velx, vely, velz);
    }
    
    public void applyVeloc(long velx, long vely, long velz)
    {
        this.Position.addVel(velx, vely, velz);
    }

    public void calculateManyForce(mass[] influences)
    {
        // Will calculate the gravitational attraction from many masses, Skips some steps to directly calculting velocity change instead of forces
        //globalVariables.log.LogTrace($"{this.ToString()} calculating forces from {influences.Length} objects");
        decimal velx = 0;
        decimal vely = 0;
        decimal velz = 0;
        decimal[] unitDir;
        // Usually magnitude is stored as a decimal but in this case it will soon be squared so need a larger cap
        double magnitude;
        decimal force;
        foreach (mass influence in influences)
        {
            if (influence == this)
            {
                continue;
            }
            // Move this check to BVolume Influence list construction once testing is done
            if (influence.Mass == 0)
            {
                continue;
            }



            unitDir = this.COM.unitVectorToo(influence.COM);
            magnitude = (double)this.COM.distanceToo(influence.COM);
            // Important that the type case to decimal happens AFTER the division as otherwise mass is allowed to be above the limit 
            // TODO There should be a check here that magnitude isnt too low
            force = checked(globalVariables.grav_const * (decimal)(influence.Mass / Math.Pow(magnitude / globalVariables.Units_in_M, 2)));

            velx += force * unitDir[0];
            vely += force * unitDir[1];
            velz += force * unitDir[2];

            //Console.WriteLine($"Influence Found: {influence.ToString()}");
            //Console.WriteLine($"{force}");
        }
        applyVeloc((long)Math.Round(velx), (long)Math.Round(vely), (long)Math.Round(velz));
    }
    
    public void update()
    {
        this.Position.update();
    }
}

public interface updateAble
{
    public void update();
    public void updateMajor();
}

public interface mass
{
    public double Mass { get; }
    public Vector COM { get; }
}
