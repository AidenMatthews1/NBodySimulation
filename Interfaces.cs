using System.Reflection;

interface updateAble
{
    public void update();
    public void updateMajor();
}

interface mass
{
    public UInt64 gravForce(Body body);
    //public UInt64 gravAttraction(Mbody body);
}