namespace RegularExpressionParser.Utils
{
    class NFAExpression
    {
        TransitionState _startingStart = null;
        TransitionState _finalState = null;

        public NFAExpression()
        {
            _startingStart = new TransitionState();
            _finalState = new TransitionState();
        }

        public NFAExpression(TransitionState stateFrom, TransitionState stateTo)
        {
            _startingStart = stateFrom;
            _finalState = stateTo;

        }

        public TransitionState StartState()
        {
            return _startingStart;
        }

        public TransitionState FinalState()
        {
            return _finalState;
        }
    }
}
