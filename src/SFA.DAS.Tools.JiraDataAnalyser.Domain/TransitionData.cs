public class TransitionData
{
    public int Transitions { get; set; }
    public int Count { get; set; }

    public TransitionData(int transitions, int count)
    {
        Transitions = transitions;
        Count = count;
    }
}