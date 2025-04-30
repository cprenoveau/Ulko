# Ulko
Quirky card game RPG with a horror twist.

Requires Unity 6.

You will find all of the main logic and objects in the Main folder and Script/Main.

The levels are held in the Timeline folder, then Act-1, Act-2 and Act-3.

Start the game from any scene (except for the ones in the Gym folder).

There's a Play from Start checkbox right beside the Unity Play/Pause/Stop buttons. Check this to start the game from the beginning, otherwise the game will start from the scene you're in.

Press F1 for cheats. You'll notice some cheats have shortcut keys, for instance Next Milestone is F12 and Win Battle is F2.

The system is divided into the following assemblies:
Main: main entry point and master of the universe, knows everything
UI: knows everything but Main
World: overworld navigation, knows Common
Battle: encounters and bosses, knows Common
Common: contains game data and shared functionalities

This game makes use of the HotChocolate library: https://cprenoveau.github.io/HotChocolate/
