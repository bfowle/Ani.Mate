Ani.Mate
========

C# port of the Ani.Mate Unity3D tweening library

------------

This lightweight, but powerful tweening library was originally written by Adrian Stutz (<adrian.stutz@gmail.com>) in Boo.
However, iOS Unity does not allow for the compilation and deployment of Boo scripts (as of October 2012).

Original source and version history notes can be found at: http://wiki.unity3d.com/index.php/AniMate


Updates
=======

- Renamed easing types to popular spellings
- Added "Spring" easing algorithm
- Various optimizations and code cleanup


Usage
=====

```c#
// create a new properties hashtable
Hashtable props = new Hashtable();

// set position: 10 units in the Y direction
props.Add("position", new Vector3(0, 10f, 0));
// set easing type: Elastic
props.Add("easing", Ani.AnimationEasingType.Elastic);
// set direction type: easeOut
props.Add("direction", Ani.EasingType.Out)

// animate the transform with the above properties in a timeframe of 2s
Ani.Mate.To(transform, 2f, props);
```


TODO
====

- Type checking optimizations to handle dynamic object types
