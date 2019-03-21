Variable and Event Objects
==========================

This is an implementation of couple of [Ryan
Hipple](https://twitter.com/roboryantron)'s [scriptable
objects](https://youtu.be/raQ3iHhE_Kk). Note that he is not affiliated with
this project.

After playing around with a basic implementation, I really liked how Hipple's
Variable Object system worked. However, this early implementation relied on
Bash scripts to automate certain editor tasks... not the best solution!

So I spent a couple months refining the editor tools for this. I think the
project is at a point where I'm happy to share it with the world. Most of this
work isn't on the original scriptable objects, but on the Unity Editor tools.

INSTALLATION
------------
Go to *Projects*, and grab the latest unitypackage. Extract this into your
project, and you're done! Alternatively, you can also clone/submodule this into
your current project using git. If you do that, I recommend putting it under a
path such as Imported/ReachBeyond/plugins/EventObjects.

In the future, I'd like to get this Unity's Asset Store. I'll put a link here
once that becomes a reality.

USAGE
-----
I recommend you watch the video linked above. It explains the gist of this
project.

I'll still go over briefly each part does. There are two parts: Event Objects
and Variable Objects. They are in the **ReachBeyond.EventObjects** and
**ReachBeyond.VariableObjects** namespaces, respectively.

### Event Objects
Event Objects are a way to send 'signals' to other components. You create
Events (i.e. EventObjects), which are ScriptableObjects. One example would be
an Event Object called "PlayerDies," which would get triggered when a player
dies. Then you'd give your player prefab an EventObjectInvoker component, and
give this your "PlayerDies" event.

Now, make a component that has a public (or serialized)
ReachBeyond.EventObjects.EventObjectListener variable. In the Unity Editor,
you'll now be able to cause something to happen using a Unity Event. This might
be respawning the player, showing a gameover screen, or something else.

This system is a bit limited, as you cannot pass values via events. I hope to
add this some time in the future.

### Variable Objects
NOTE: THIS PERTAINS TO ANOTHER REPO. I'll clean this up in the future.

This is where most of the work has gone in. Variable objects allow you to track
values outside of specific components. In this sense, they are global. However,
they give the Unity Editor control over what objects can access these
components.

I won't describe these in much detail for the time being. I highly recommend
watching the video linked above for the premise. I'll mostly be going over what
I've added.

Variable Objects inherit from Event Objects. Whenever the value of a Variable
Object changes, it emits an event. This lets you create Listeners that trigger
when a value gets updated.


There are components called **Registraters**. These allow you to make
'singletons' using variable objects. They're ideal for prefabs that you only
want to exist one time in the scene, such as a player, a boss, or HUD elements.

In the future, I want to create a variation of this which works with lists,
making objects automatically add/remove themselves to/from a list. Also,
registraters only work with reference datatypes (i.e. classes).


If you go to the **Window** dropdown, you'll find a couple of buttons related
to Variable Objects. One will let you view/manage all of the variable object types
supported, while the other lets you create new ones.

You normally shouldn't need to create new variable objects unless you need to
keep track of variables using your own datatypes. (If you do use this feature,
make sure you read all the tooltips and hints. If you don't, you'll get syntax
errors.)

These editor tools require tags to work correctly. If you need to move files
around, make sure the tags are preserved!



OTHER THINGS
------------
More documentation will come eventually.

If you have a bug to report or a feature request, please open an issue!

For the LICENSE, it applies to most things. Both Variable Objects and Event
Objects are under the LGPL. (Hipple has said that he considers the stuff in
the videos to be open-source. LGPL allows the code to be open source while
still allowing gamedevs to use the project in a closed project.)

However, the icons aren't my own; they come from
[www.flaticon.com](www.flaticon.com).  In every icons folder, there is a
credits file. These credits files comply with flaticon's license. Keep this in
mind if you plan on using these icons for your own projects.
