using System;
using System.Collections.Generic;
using System.Linq;

namespace ShablonD7
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class Light
    {
        public string Name { get; }
        private bool _isOn;
        public Light(string name) => Name = name;
        public void On() { _isOn = true; Console.WriteLine($"[Light] {Name}: ON"); }
        public void Off() { _isOn = false; Console.WriteLine($"[Light] {Name}: OFF"); }
    }

    public class Door
    {
        public string Name { get; }
        private bool _isOpen;
        public Door(string name) => Name = name;
        public void Open() { _isOpen = true; Console.WriteLine($"[Door] {Name}: OPEN"); }
        public void Close() { _isOpen = false; Console.WriteLine($"[Door] {Name}: CLOSED"); }
    }

    public class Thermostat
    {
        public string Name { get; }
        private int _temperature;
        public Thermostat(string name, int initial = 22) { Name = name; _temperature = initial; }
        public void Increase(int delta) { _temperature += delta; Console.WriteLine($"[Thermostat] {Name}: {_temperature}°C"); }
        public void Decrease(int delta) { _temperature -= delta; Console.WriteLine($"[Thermostat] {Name}: {_temperature}°C"); }
        public void Set(int temp) { _temperature = temp; Console.WriteLine($"[Thermostat] {Name}: set to {_temperature}°C"); }
    }

    public class LightOnCommand : ICommand
    {
        private readonly Light _light;
        public LightOnCommand(Light light) => _light = light;
        public void Execute() => _light.On();
        public void Undo() => _light.Off();
    }

    public class LightOffCommand : ICommand
    {
        private readonly Light _light;
        public LightOffCommand(Light light) => _light = light;
        public void Execute() => _light.Off();
        public void Undo() => _light.On();
    }

    public class DoorOpenCommand : ICommand
    {
        private readonly Door _door;
        public DoorOpenCommand(Door door) => _door = door;
        public void Execute() => _door.Open();
        public void Undo() => _door.Close();
    }

    public class DoorCloseCommand : ICommand
    {
        private readonly Door _door;
        public DoorCloseCommand(Door door) => _door = door;
        public void Execute() => _door.Close();
        public void Undo() => _door.Open();
    }

    public class ThermostatIncreaseCommand : ICommand
    {
        private readonly Thermostat _thermostat;
        private readonly int _delta;
        public ThermostatIncreaseCommand(Thermostat t, int delta = 1) { _thermostat = t; _delta = delta; }
        public void Execute() => _thermostat.Increase(_delta);
        public void Undo() => _thermostat.Decrease(_delta);
    }

    public class ThermostatDecreaseCommand : ICommand
    {
        private readonly Thermostat _thermostat;
        private readonly int _delta;
        public ThermostatDecreaseCommand(Thermostat t, int delta = 1) { _thermostat = t; _delta = delta; }
        public void Execute() => _thermostat.Decrease(_delta);
        public void Undo() => _thermostat.Increase(_delta);
    }

    public class Alarm
    {
        public string Name { get; }
        private bool _armed;
        public Alarm(string name) => Name = name;
        public void Arm() { _armed = true; Console.WriteLine($"[Alarm] {Name}: ARMED"); }
        public void Disarm() { _armed = false; Console.WriteLine($"[Alarm] {Name}: DISARMED"); }
    }

    public class AlarmArmCommand : ICommand
    {
        private readonly Alarm _alarm;
        public AlarmArmCommand(Alarm alarm) => _alarm = alarm;
        public void Execute() => _alarm.Arm();
        public void Undo() => _alarm.Disarm();
    }

    public class AlarmDisarmCommand : ICommand
    {
        private readonly Alarm _alarm;
        public AlarmDisarmCommand(Alarm alarm) => _alarm = alarm;
        public void Execute() => _alarm.Disarm();
        public void Undo() => _alarm.Arm();
    }

    public class MacroCommand : ICommand
    {
        private readonly IList<ICommand> _commands;
        public MacroCommand(IEnumerable<ICommand> commands) => _commands = commands.ToList();
        public void Execute()
        {
            foreach (var c in _commands) c.Execute();
        }
        public void Undo()
        {
            for (int i = _commands.Count - 1; i >= 0; i--) _commands[i].Undo();
        }
    }

    public class Invoker
    {
        private readonly Dictionary<int, ICommand> _slots = new();
        private readonly Stack<ICommand> _history = new();
        private readonly int _maxHistory;

        public Invoker(int maxHistory = 20) { _maxHistory = Math.Max(1, maxHistory); }

        public void SetCommand(int slot, ICommand command) => _slots[slot] = command;

        public void ExecuteSlot(int slot)
        {
            if (!_slots.TryGetValue(slot, out var cmd))
            {
                Console.WriteLine($"[Invoker] Слот {slot} не назначен.");
                return;
            }

            try
            {
                cmd.Execute();
                _history.Push(cmd);
                if (_history.Count > _maxHistory)
                {
                    var arr = _history.Reverse().Skip(1).Reverse().ToArray();
                    _history.Clear();
                    foreach (var h in arr) _history.Push(h);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Invoker] Ошибка при выполнении команды: {ex.Message}");
            }
        }

        public void Undo()
        {
            if (_history.Count == 0)
            {
                Console.WriteLine("[Invoker] Нет команд для отмены.");
                return;
            }
            var cmd = _history.Pop();
            try
            {
                cmd.Undo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Invoker] Ошибка при отмене: {ex.Message}");
            }
        }

        public void ShowHistory()
        {
            Console.WriteLine("[Invoker] История (последние выполненные команды):");
            if (_history.Count == 0) { Console.WriteLine("  (пусто)"); return; }
            foreach (var cmd in _history) Console.WriteLine($"  - {cmd.GetType().Name}");
        }
    }

    public abstract class Beverage
    {
        public void PrepareRecipe()
        {
            BoilWater();
            BrewOrSteep();
            PourInCup();
            if (CustomerWantsCondiments())
                AddCondiments();
            AfterPrepareHook();
        }

        protected abstract void BrewOrSteep();
        protected abstract void AddCondiments();

        protected virtual void BoilWater() => Console.WriteLine("Кипячение воды...");
        protected virtual void PourInCup() => Console.WriteLine("Наливание в чашку...");
        protected virtual bool CustomerWantsCondiments() => true;
        protected virtual void AfterPrepareHook() { }
    }

    public class Tea : Beverage
    {
        protected override void BrewOrSteep() => Console.WriteLine("Заваривание чая 3 минуты...");
        protected override void AddCondiments() => Console.WriteLine("Добавление лимона и мёда...");
        protected override bool CustomerWantsCondiments()
        {
            Console.Write("Хотите добавки для чая? (y/n): ");
            var ans = Console.ReadLine()?.Trim().ToLower();
            return ans == "y" || ans == "yes";
        }
    }

    public class Coffee : Beverage
    {
        protected override void BrewOrSteep() => Console.WriteLine("Заваривание кофе в кофемашине...");
        protected override void AddCondiments() => Console.WriteLine("Добавление молока и сахара...");
        protected override bool CustomerWantsCondiments()
        {
            Console.Write("Хотите добавки для кофе? (y/n): ");
            var ans = Console.ReadLine()?.Trim().ToLower();
            return ans == "y" || ans == "yes";
        }
        protected override void AfterPrepareHook() => Console.WriteLine("Кофе готов — наслаждайтесь!");
    }

    public interface IMediator
    {
        void Register(User user, string room);
        void Unregister(User user, string room);
        void SendRoomMessage(string room, User from, string message);
        void SendPrivateMessage(User from, User to, string message);
        IEnumerable<string> ListRooms();
    }

    public class ChatRoomMediator : IMediator
    {
        private readonly Dictionary<string, HashSet<User>> _rooms = new();

        public void Register(User user, string room)
        {
            if (!_rooms.ContainsKey(room)) _rooms[room] = new HashSet<User>();
            if (_rooms[room].Add(user))
            {
                BroadcastSystem(room, $"{user.Name} присоединился к {room}");
            }
        }

        public void Unregister(User user, string room)
        {
            if (_rooms.ContainsKey(room) && _rooms[room].Remove(user))
            {
                BroadcastSystem(room, $"{user.Name} покинул {room}");
            }
        }

        public void SendRoomMessage(string room, User from, string message)
        {
            if (!_rooms.ContainsKey(room))
            {
                Console.WriteLine($"[Mediator] Комната {room} не найдена.");
                return;
            }
            if (!_rooms[room].Contains(from))
            {
                Console.WriteLine($"[Mediator] {from.Name} не состоит в комнате {room}.");
                return;
            }
            foreach (var u in _rooms[room])
                if (!u.Equals(from))
                    u.ReceiveRoom(room, from.Name, message);
        }

        public void SendPrivateMessage(User from, User to, string message)
        {
            if (to == null) { Console.WriteLine("[Mediator] Получатель не найден."); return; }
            to.ReceivePrivate(from.Name, message);
        }

        private void BroadcastSystem(string room, string text)
        {
            if (!_rooms.ContainsKey(room)) return;
            foreach (var u in _rooms[room]) u.ReceiveRoom(room, "SYSTEM", text);
        }

        public IEnumerable<string> ListRooms() => _rooms.Keys;
    }

    public class User
    {
        public string Name { get; }
        private readonly IMediator _mediator;
        public User(string name, IMediator mediator) { Name = name; _mediator = mediator; }

        public void Join(string room) => _mediator.Register(this, room);
        public void Leave(string room) => _mediator.Unregister(this, room);
        public void SendToRoom(string room, string message) => _mediator.SendRoomMessage(room, this, message);
        public void SendPrivate(User to, string message) => _mediator.SendPrivateMessage(this, to, message);

        public void ReceiveRoom(string room, string from, string message) => Console.WriteLine($"[{room}] {from} -> {Name}: {message}");
        public void ReceivePrivate(string from, string message) => Console.WriteLine($"[Private] {from} -> {Name}: {message}");
    }


    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Command Pattern (Smart Home) Demo ===");
            RunCommandDemo();

            Console.WriteLine("\n=== Template Method (Beverage) Demo ===");
            RunTemplateDemo();

            Console.WriteLine("\n=== Mediator (Chat) Demo ===");
            RunMediatorDemo();

            Console.WriteLine("\nГотово. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void RunCommandDemo()
        {
            var livingLight = new Light("LivingRoom");
            var hallDoor = new Door("FrontDoor");
            var thermo = new Thermostat("MainThermo");
            var alarm = new Alarm("HomeAlarm");

            var invoker = new Invoker(maxHistory: 10);

            invoker.SetCommand(1, new LightOnCommand(livingLight));
            invoker.SetCommand(2, new LightOffCommand(livingLight));
            invoker.SetCommand(3, new DoorOpenCommand(hallDoor));
            invoker.SetCommand(4, new DoorCloseCommand(hallDoor));
            invoker.SetCommand(5, new ThermostatIncreaseCommand(thermo, 2));
            invoker.SetCommand(6, new ThermostatDecreaseCommand(thermo, 2));
            invoker.SetCommand(7, new AlarmArmCommand(alarm));
            invoker.SetCommand(8, new AlarmDisarmCommand(alarm));

            invoker.ExecuteSlot(1); 
            invoker.ExecuteSlot(5);
            invoker.ExecuteSlot(3); 

            invoker.Undo();
            invoker.Undo();

            var macro = new MacroCommand(new ICommand[] {
                new LightOnCommand(livingLight),
                new ThermostatDecreaseCommand(thermo, 2),
                new AlarmArmCommand(alarm)
            });
            invoker.SetCommand(10, macro);
            invoker.ExecuteSlot(10);

            invoker.Undo();

            invoker.ShowHistory();

            while (true)
            {
                Console.Write("Отменить ещё? (y/n): ");
                var a = Console.ReadLine()?.Trim().ToLower();
                if (a == "y") invoker.Undo();
                else break;
            }
        }

        static void RunTemplateDemo()
        {
            Console.WriteLine("\n--- Tea ---");
            var tea = new Tea();
            tea.PrepareRecipe();

            Console.WriteLine("\n--- Coffee ---");
            var coffee = new Coffee();
            coffee.PrepareRecipe();
        }

        static void RunMediatorDemo()
        {
            var mediator = new ChatRoomMediator();

            var alice = new User("Alice", mediator);
            var bob = new User("Bob", mediator);
            var ann = new User("Ann", mediator);

            alice.Join("general");
            bob.Join("general");
            ann.Join("random");

            alice.SendToRoom("general", "Hello everyone!");
            bob.SendToRoom("general", "Hi Alice!");

            alice.SendPrivate(bob, "Привет! Это приватное сообщение.");

            bob.SendToRoom("random", "Am I in random?"); 

            ann.Join("general");
            ann.SendToRoom("general", "Теперь я тут тоже.");

            bob.Leave("general");
            alice.SendToRoom("general", "Bob left?");

            Console.WriteLine("Комнаты: " + string.Join(", ", mediator.ListRooms()));
        }
    }
}
