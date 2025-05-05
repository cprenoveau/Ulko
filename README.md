# Ulko
Quirky card game RPG with a horror twist.

Requires Unity 6.

You will find all of the main logic and objects in the Main folder and Scripts/Main.

The levels are held in the Timeline folder which contains chapters.

Start the game from any scene (except for the ones in the Gym folder).

There's a Play from Start checkbox right beside the Unity Play/Pause/Stop buttons. Check this to start the game from the beginning, otherwise the game will start from the scene you're in.

Press F1 for cheats. You'll notice some cheats have shortcut keys, for instance Next Milestone is F12 and Win Battle is F2.

The system is divided into the following assemblies:

Main: main entry point and master of the universe, knows everything<br>
UI: knows everything but Main<br>
World: overworld navigation, knows Common<br>
Battle: encounters and bosses, knows Common<br>
Common: contains game data and shared functionalities<br>

This game makes use of the HotChocolate library: https://cprenoveau.github.io/HotChocolate/
