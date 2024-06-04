using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper;

/// <summary>
/// A multi-thread input system to use instead of blocking single-thread <see cref="Console"/>'s methods
/// </summary>
    public static class InputSystem
    {
        public static event EventHandler<ConsoleKeyInfo>? KeyDown;
        private static ManualResetEventSlim _keyDownHandle = null!;
        private static object _waitingLock = null!;
        private static int _waitingThreads;
        private static ConsoleKeyInfo _lastWaitedKey;

        private static bool _initialized = false;

        private static bool TryInitialize()
        {
            if(_initialized)
                return false;

            _initialized = true;
            
            _lastWaitedKey = new();
            _waitingLock = new();
            _waitingThreads = 0;
            _keyDownHandle = new(false);
            return true;
        }

        /// <summary>
        /// Runs the input system loop syncronously, turning this thread in the "Logic/Input thread".<br/>
        /// It is reccomended to run this in the main thread.
        /// </summary>
        public static void Run()
        {
            TryInitialize();

            while(true)
            {
                ConsoleKeyInfo info = Console.ReadKey(true);
                KeyDown?.Invoke(null, info);

                // If no threads are waiting skip handling
                lock(_waitingLock)
                    if(_waitingThreads == 0)
                        continue;

                // Can set this syncronously as no thread will read it until
                // The handle down here is set.
                _lastWaitedKey = info;
                _keyDownHandle.Set();

                int waiting;
                
                // Block this thread until all other threads have stopped waiting
                // This makes sure that no variable will be changed by this method until "_waitingThreads" reaches 0
                do lock(_waitingLock)
                    waiting = _waitingThreads;
                while(waiting > 0);

                // Close handle so new threads wait.
                _keyDownHandle.Reset();
            }
        }

        /// <summary>
        /// Stops the input loop and deallocates all resources utilized.
        /// </summary>
        public static void Stop()
        {
            if(!_initialized)
                return;

            _initialized = false;

            _waitingLock = null!;
            _waitingThreads = 0;

            _keyDownHandle.Dispose();
            _keyDownHandle = null!;
        }

        /// <summary>
        /// Similar behaviour to <see cref="Console.ReadKey"/><br/>
        /// Waits until a key is pressed and returns it <c>without</c> putting it to the screen<br/>
        /// </summary>
        /// <remarks><b>Note: </b> The calling thread is blocked until any key is pressed</remarks>
        /// <returns>A <see cref="ConsoleKeyInfo"/> containing information about the character, key and modifiers pressed</returns>
        /// <exception cref="InvalidOperationException"/>
        public static ConsoleKeyInfo ReadKey()
            => ReadKey(CancellationToken.None);

        /// <summary>
        /// Similar behaviour to <see cref="Console.ReadKey"/><br/>
        /// Waits until a key is pressed and returns it <c>without</c> putting it to the screen<br/>
        /// </summary>
        /// <remarks><b>Note: </b> The calling thread is blocked until any key is pressed</remarks>
        /// <returns>A <see cref="ConsoleKeyInfo"/> containing information about the character, key and modifiers pressed</returns>
        /// <exception cref="InvalidOperationException"/>
        public static ConsoleKeyInfo ReadKey(CancellationToken token)
        {
            // Notify subscription
            lock(_waitingLock)
                _waitingThreads++;

            // Wait for key press
            try {
                _keyDownHandle.Wait(token);
            }
            catch(OperationCanceledException){

            }

            // Can safely copy the key as "Run" will not change it
            // until "_waitingThreads" reaches 0.
            ConsoleKeyInfo keyPressed = _lastWaitedKey;

            // Notify unsubscription
            lock(_waitingLock)
                _waitingThreads--;

            // Return the *copied* key
            // Can't return "_lastWaitedKey" as this thread already unsubscribed.
            // If this was the last to unsubscribe, the value of "_lastWaitedKey" may have been already changed
            return keyPressed;
        }
    }