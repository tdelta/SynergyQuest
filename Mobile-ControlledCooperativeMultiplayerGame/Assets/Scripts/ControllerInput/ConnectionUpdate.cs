using System;

/**
 * The `ControllerConnection` class handles all communication between the game and a controller client on a low level on
 * another thread.
 * It communicates with the main thread via message passing and this class hierarchy determines the shape of those
 * messages. See the `ControllerConnection` and `ControllerServer` classes.
 *
 * All concrete kinds of messages are subclasses of `ConnectionUpdate`.
 * One can do a "switch-case" on them using the `Match` method. See the `ControllerServer` class for an example
 * 
 * This implementation simulates an ADT as known from functional programming languages like Haskell or object-oriented
 * languages with integration of functional concepts (Kotlin, Scala).
 * If we get tired of simulating ADTs in C#, we maybe could use F# instead: https://www.davideaversa.it/blog/quick-look-at-f-in-unity/
 *
 * The point is that ADTs are well suited for modelling different kinds of messages.
 */
public abstract class ConnectionUpdate
{
    /**
     * Every websocket connection gets assigned a numerical connection id upon initialization by `ControllerServer`.
     * It is send with every inter-thread message to identify from which connection it is coming.
     *
     * (websocket-sharp already implements string based connection IDs, but do not use them for the most time to avoid
     *  frequent string comparisons)
     */
    public readonly int connectionId;

    /**
     * This inter-thread message is sent, whenever a controller client sends the name of its player to establish a connection.
     */
    public sealed class NameUpdate : ConnectionUpdate
    {
        // Name of the player
        public readonly string name;
        // websocket-sharp internal websocket id. The ControllerServer will store it to send messages to a specific websocket.
        public readonly string websocketId;

        public NameUpdate(int connectionId, string name, string websocketId)
            : base(connectionId)
        {
            this.name = name;
            this.websocketId = websocketId;
        }
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.NameUpdate(this);
        }
    }
    
    /**
     * This inter-thread message informs the `ControllerServer` that a message from a controller has arrived over the network
     */
    public sealed class MessageUpdate : ConnectionUpdate
    {
        // Network message that arrived
        public readonly Message message;

        public MessageUpdate(int connectionId, Message msg)
            : base(connectionId)
        {
            this.message = msg;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.MessageUpdate(this);
        }
    }

    /**
     * This inter-thread message informs the `ControllerServer` that a websocket disconnected.
     */
    public sealed class DisconnectUpdate : ConnectionUpdate
    {
        public DisconnectUpdate(int connectionId)
            : base(connectionId)
        { }
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.DisconnectUpdate(this);
        }
    }
    
    private ConnectionUpdate(int connectionId)
    {
        this.connectionId = connectionId;
    }
    
    /**
     * Applying some sort of switch-case on the different inter-thread message types is cumbersome since it requires
     * dynamic typechecks, typecasts or visitor pattern etc.
     *
     * Here instead, we simulate a "Match" functionality as it is used in functional
     * programming languages like Haskell or Scala for ADTs.
     *
     * See the `Update` method of `ControllerServer` for an usage example.
     *
     * @param matcher contains a callback action for each kind of message.
     */
    public abstract void Match( Matcher matcher );
    
    /**
     * See `Match` method.
     */
    public sealed class Matcher
    {
        public Action<NameUpdate> NameUpdate = _ => {};
        public Action<MessageUpdate> MessageUpdate = _ => { };
        public Action<DisconnectUpdate> DisconnectUpdate = _ => {};
    }
}
