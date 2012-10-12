/**
 * AniMate animation helper class for Unity3D
 * Version 2.0 - 9. October 2009
 * Copyright (C) 2009  Adrian Stutz
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, see <http://www.gnu.org/licenses/>.
 *
 * ============================================================
 * C# Conversion by Brett Fowle <http://bfowle.com/> July 6, 2012
 *
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class Ani : MonoBehaviour {

    public enum AniType {
        To,
        From,
        By
    }

    public delegate void DefaultCallback(System.Object val);

    // ---------------------------------------- //
    // CONFIGURATION PROPERTIES

    // Default delay
    public float defaultDelay = 0;
    // Default physics behaviour
    public bool defaultPhysics = false;
    // Default callback
    public DefaultCallback defaultCallback = null;
    // Default easing
    public AnimationEasingType defaultEasing = AnimationEasingType.Linear;
    // Default easing direction
    public EasingType defaultDirection = EasingType.In;
    // Default animation drive
    public AnimationDriveType defaultDrive = AnimationDriveType.Regular;
    // Default frames per second (-1 for fullspeed)
    public float defaultFps = -1f;
    // Remove existing animations for property
    public bool defaultReplace = false;

    // ---------------------------------------- //
    // INTERNAL FIELDS

    private List<AniProp> animations = new List<AniProp>(32);
    private List<AniProp> fixedAnimations = new List<AniProp>(32);
    //
    Hashtable properties, options, exct;
    List<AniProp> list, temp;
    int i, len;
    float spf;

    // ---------------------------------------- //
    // SINGLETON

    // Singleton instance
    private static Ani _mate;
    public static Ani Mate {
        get {
            // Create instance if none exists yet
            if (_mate == null) {
                // Create GameObject to attach to
                GameObject go = new GameObject("_AniMate");
                // Attach Ani to GameObject
                _mate = (Ani)go.AddComponent<Ani>();
            }
            return _mate;
        }
    }

    // Save instance
    public void Awake() {
        if (Ani._mate) {
            return;
        }
        Ani._mate = this;
    }

    // ---------------------------------------- //
    // CREATE NEW ANIMATION

    public WaitForSeconds To(UnityEngine.Object obj, float duration, Hashtable _properties) {
        properties = (Hashtable)_properties.Clone();
        options = ExtractOptions(ref properties);
        CreateAnimations(obj, properties, duration, options, AniType.To);
        return new WaitForSeconds(duration);
    }

    public WaitForSeconds To(System.Object obj, float duration, Hashtable _properties, Hashtable _options) {
        properties = (Hashtable)_properties.Clone();
        options = (Hashtable)_options.Clone();
        options = ExtractOptions(ref options);
        CreateAnimations(obj, properties, duration, options, AniType.To);
        return new WaitForSeconds(duration);
    }

    public WaitForSeconds From(System.Object obj, float duration, Hashtable _properties) {
        properties = (Hashtable)_properties.Clone();
        options = ExtractOptions(ref properties);
        CreateAnimations(obj, properties, duration, options, AniType.From);
        return new WaitForSeconds(duration);
    }

    public WaitForSeconds From(System.Object obj, float duration, Hashtable _properties, Hashtable _options) {
        properties = (Hashtable)_properties.Clone();
        options = (Hashtable)_options.Clone();
        options = ExtractOptions(ref options);
        CreateAnimations(obj, properties, duration, options, AniType.From);
        return new WaitForSeconds(duration);
    }

    public WaitForSeconds By(System.Object obj, float duration, Hashtable _properties) {
        properties = (Hashtable)_properties.Clone();
        options = ExtractOptions(ref properties);
        CreateAnimations(obj, properties, duration, options, AniType.By);
        return new WaitForSeconds(duration);
    }

    public WaitForSeconds By(System.Object obj, float duration, Hashtable _properties, Hashtable _options) {
        properties = (Hashtable)_properties.Clone();
        options = (Hashtable)_options.Clone();
        options = ExtractOptions(ref options);
        CreateAnimations(obj, properties, duration, options, AniType.By);
        return new WaitForSeconds(duration);
    }

    // ---------------------------------------- //
    // MANAGE ANIMATIONS

    // Number of all animations
    public int Count() {
        return (animations.Count + fixedAnimations.Count);
    }

    // Check if an animation exists for object
    public bool Has(System.Object obj) {
        return (Contains(obj, "", ref animations) ||
                Contains(obj, "", ref fixedAnimations));
    }

    // Check if animation exists for object and proeperty
    public bool Has(System.Object obj, string name) {
        return (Contains(obj, name, ref animations) ||
                Contains(obj, name, ref fixedAnimations));
    }

    // Check for object and property
    private bool Contains(System.Object obj, string name, ref List<AniProp> anims) {
        foreach (AniProp anim in anims) {
            if ((name == null && anim.value.Is(obj)) ||
                (name != null && anim.value.Is(obj, name))) {
                return true;
            }
        }
        return false;
    }

    // Cancel all animations on an object
    public void Cancel(System.Object obj) {
        Cancel(obj, null);
    }

    // Cancel animation (set to initial value)
    public void Cancel(System.Object obj, string name) {
        list = GetAnimations(obj, name);
        foreach (AniProp anim in list) {
            Apply(0, anim, true);
            animations.Remove(anim);
            fixedAnimations.Remove(anim);
        }
    }

    // Finish all animations on an object
    public void Finish(System.Object obj) {
        Finish(obj, null);
    }

    // Finish animation (set to end value)
    public void Finish(System.Object obj, string name) {
        list = GetAnimations(obj, name);
        foreach (AniProp anim in list) {
            Apply(1, anim, true);
            animations.Remove(anim);
            fixedAnimations.Remove(anim);
        }
    }

    // Stop all animations of an object
    public void StopAll(System.Object obj) {
        Stop(obj, null, null);
    }

    // Stop all animations of an object for a property
    public void Stop(System.Object obj, string name) {
        Stop(obj, name, null);
    }

    // Remove animations
    private void Stop(System.Object obj, string name, AniProp exclude) {
        list = GetAnimations(obj, name);
        foreach (AniProp anim in list) {
            if (exclude == anim) {
                continue;
            }

            animations.Remove(anim);
            fixedAnimations.Remove(anim);
        }
    }

    // Get Animation Lists
    private List<AniProp> GetAnimations(System.Object obj, string name) {
        temp = new List<AniProp>(32);

        // Look in regular animations
        foreach (AniProp anim in animations) {
            if ((name == null && anim.value.Is(obj)) ||
                (name != null && anim.value.Is(obj, name))) {
                temp.Add(anim);
            }
        }

        // Look in fixed animations
        foreach (AniProp anim in fixedAnimations) {
            if ((name == null && anim.value.Is(obj)) ||
                (name != null && anim.value.Is(obj, name))) {
                temp.Add(anim);
            }
        }

        return temp;
    }

    // ---------------------------------------- //
    // MAIN ANIMATION LOOPS

    private void DoAnimation(ref List<AniProp> anims) {
        list = new List<AniProp>(32);

        // Loop through animations
        for (i = 0, len = anims.Count; i < len; i++) {
            if (i >= anims.Count) {
                continue;
            }

            AniProp anim = anims[i];
            DefaultCallback callback = (anim.callback as DefaultCallback);
            if (!anim.mator.Running()) {
                continue;
            }
            anim.value.Set(anim.mator.GetValue());

            if (callback != null) {
                callback(anim.mator.GetValue());
            }
            if (anim.mator.Finished()) {
                list.Add(anim);
            }
        }

        // Remove finished animations
        foreach (AniProp fin in list) {
            anims.Remove(fin);
        }
    }

    private bool Apply(float position, AniProp anim, bool forceUpdate) {
        spf = (float)anim.options["fps"];

        // Ignore restrictions if forced
        if (!forceUpdate) {
            // Honor seconds per frame
            if (spf > 0) {
                anim.timeSinceLastFrame += Time.deltaTime;
                // Not yet time, skip
                if (anim.timeSinceLastFrame < spf) {
                    return true;
                } else {
                    // Update this frame
                    anim.timeSinceLastFrame = anim.timeSinceLastFrame % spf;
                }
            }
        }

        // Animate or call calback with value
        try {
            if (anim.callback == null) {
                anim.value.Set(anim.mator.GetValue(position));
            } else {
                anim.callback(anim.mator.GetValue(position));
            }
        } catch {
        }

        // Check if finished
        if (anim.mator.Finished()) {
            return false;
        }

        return true;
    }

    // Regular animations
    public void LateUpdate() {
        DoAnimation(ref animations);
    }

    // Physics animations
    public void FixedUpdate() {
        DoAnimation(ref fixedAnimations);
    }

    // ---------------------------------------- //
    // INTERNAL METHODS

    // Exctract options for Hash and fill defaults where needed
    private Hashtable ExtractOptions(ref Hashtable options) {
        exct = new Hashtable();

        // Delay
        if (options["delay"] == null) {
            exct["delay"] = defaultDelay;
        } else {
            exct["delay"] = (float)options["delay"];
            options.Remove("delay");
        }

        // Physics
        if (options["physics"] == null) {
            exct["physics"] = defaultPhysics;
        } else {
            exct["physics"] = (bool)options["physics"];
            options.Remove("physics");
        }

        // Callback
        if (options["callback"] == null) {
            exct["callback"] = defaultCallback;
        } else {
            exct["callback"] = (DefaultCallback)options["callback"];
            options.Remove("callback");
        }

        // Easing
        if (options["easing"] == null) {
            exct["easing"] = defaultEasing;
        } else {
            exct["easing"] = (AnimationEasingType)options["easing"];
            options.Remove("easing");
        }

        // Easing Direction
        if (options["direction"] == null) {
            exct["direction"] = defaultDirection;
        } else {
            exct["direction"] = (EasingType)options["direction"];
            options.Remove("direction");
        }

        // Animation drive
        if (options["drive"] == null) {
            exct["drive"] = defaultDrive;
        } else {
            exct["drive"] = (AnimationDriveType)options["drive"];
            options.Remove("drive");
        }

        // Rigidbody animation
        if (options["rigidbody"] == null) {
            exct["rigidbody"] = null;
        } else {
            exct["rigidbody"] = (Rigidbody)options["rigidbody"];
            options.Remove("rigidbody");
        }

        // Color animation
        if (options["colorName"] == null) {
            exct["colorName"] = null;
        } else {
            exct["colorName"] = (string)options["colorName"];
            options.Remove("colorName");
        }

        // Fps (saved as seconds per frame)
        if (options["fps"] == null) {
            exct["fps"] = 1 / defaultFps;
        } else {
            exct["fps"] = 1 / (float)options["fps"];
            options.Remove("fps");
        }

        // Replace animation on property
        if (options["replace"] == null) {
            exct["replace"] = defaultReplace;
        } else {
            exct["replace"] = (bool)options["replace"];
            options.Remove("replace");
        }

        // Return hash with all values
        return exct;
    }

    // Extract animation properties from Hash
    private void CreateAnimations(System.Object obj, Hashtable properties, float duration,
        Hashtable options, AniType type) {

        foreach (DictionaryEntry item in properties) {
            // Extract name and value
            string name = (string)item.Key;
            System.Object value = item.Value;
            // Create value object
            AniValue aniv = new AniValue(obj, name);
            // Get current value
            System.Object current = aniv.Get();

            System.Object start = null;
            System.Object target = null;
            System.Object diff = null;

            // Setup variables
            if (type == AniType.To) {
                start = current;
                target = value;
            } else if (type == AniType.From) {
                start = value;
                target = current;
            } else if (type == AniType.By) {
                start = current;
                diff = value;
            }

            // Cast value to destination type
            System.Object argument = System.Convert.ChangeType(item.Value, aniv.ValueType());
            // Callback
            DefaultCallback callback = (DefaultCallback)options["callback"];

            // Calculate difference for To and From
            if ((type == AniType.To ||
                 type == AniType.From) &&
                DriveNeedsDiff((AnimationDriveType)options["drive"])) {

                try {
                    //diff = target - start;
                    //test = start + 0.1 * diff;

                    System.Type startType = start.GetType();

                    // --- Builtin types
                    if (startType != target.GetType()) {
                        diff = (float)target - (float)start;
                    } else if (startType == typeof(short)) {
                        diff = (short)target - (short)start;
                    } else if (startType == typeof(int)) {
                        diff = (int)target - (int)start;
                    } else if (startType == typeof(long)) {
                        diff = (long)target - (long)start;
                    } else if (startType == typeof(float)) {
                        diff = (float)target - (float)start;
                    } else if (startType == typeof(double)) {
                        diff = (double)target - (double)start;
                    } else if (startType == typeof(decimal)) {
                        diff = (decimal)target - (decimal)start;
                    // --- Unity types
                    } else if (startType == typeof(Vector2)) {
                        diff = (Vector2)target - (Vector2)start;
                    } else if (startType == typeof(Vector3)) {
                        diff = (Vector3)target - (Vector3)start;
                    } else if (startType == typeof(Vector4)) {
                        diff = (Vector4)target - (Vector4)start;
                    } else if (startType == typeof(Color)) {
                        diff = (Color)target - (Color)start;
                    // --- Fallback
                    } else {
                        diff = (float)target - (float)start;
                    }
                } catch {
                    throw new System.Exception("Cannot find diff between " + start.GetType() + " and " + target.GetType() + ": Operation +, - or * not supported.");
                }
            }

            // Start time
            float startTime = 0;
            float delay = (float)options["delay"];
            if (delay > 0) {
                startTime = Time.time + delay;
            }

            // Create animation object
            AniMator mat = new AniMator(start, target, diff, duration, (float)options["delay"], (AnimationEasingType)options["easing"],
                (EasingType)options["direction"], (AnimationDriveType)options["drive"]);

            // Add animation to main list
            AniProp anim = new AniProp(aniv, mat, type, duration, callback, startTime, argument, options);
            // Regular animation
            animations.Add(anim);

            // From: Set to starting value
            if (type == AniType.From) {
                aniv.Set(start);
            }
        }
    }

    private bool DriveNeedsDiff(AnimationDriveType drive) {
        AnimationDrive d = new AnimationDrive(drive);
        return d.CalculateDiff();
    }

    private void CreateMaterialColorCallback(Material obj, string name, Hashtable options) {
        float newValue = 0;
        Material material = obj;
        Color newColor = material.GetColor((string)options["colorName"]);

        if (name == "r") {
            newColor.r = newValue;
        } else if (name == "g") {
            newColor.g = newValue;
        } else if (name == "b") {
            newColor.b = newValue;
        } else {
            newColor.a = newValue;
        }
        material.SetColor((string)options["colorName"], newColor);
    }

    // ---------------------------------------- //
    // CONTAINER CLASS FOR ANIMATION PROPERTIES

    public class AniProp {
        // ---------------------------------------- //
        // CONFIGURATION

        public AniValue value;
        public AniMator mator;
        public AniType type;
        public float duration;

        public DefaultCallback callback;
        public float timeSinceLastFrame;
        public System.Object argument;
        public float startTime;
        public Hashtable options;

        // ---------------------------------------- //
        // CONSTRUCTOR

        public AniProp(AniValue v, AniMator m, AniType t, float d, DefaultCallback c, float s, System.Object a, Hashtable o) {
            value = v;
            mator = m;
            type = t;
            duration = d;
            callback = c;
            startTime = s;
            argument = a;
            options = o;
        }
    }

    // ---------------------------------------- //
    // WRAPPER FOR A SINGLE VALUE

    public class AniValue {
        // ---------------------------------------- //
        // CONFIGURATION

        static BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static;

        public delegate void SetHandler(System.Object source, System.Object value);

        // ---------------------------------------- //
        // PRIVATE FIELDS

        // Object a field or property is animated on
        public System.Object obj;
        // Name of the field or property
        public string name;

        // Type object
        public System.Type objType;
        // FieldInfo object
        public FieldInfo fieldInfo;
        // PropertyInfo object
        public PropertyInfo propertyInfo;

        // ---------------------------------------- //
        // CONSTRUCTOR

        public AniValue(System.Object o, string n) {
            // Save
            obj = o;
            name = n;

            // Get info objects
            objType = obj.GetType();

            // Get field or property info
            fieldInfo = objType.GetField(n, AniValue.bFlags);
            propertyInfo = objType.GetProperty(n, AniValue.bFlags);

            // Check info objects
            if (fieldInfo == null &&
                propertyInfo == null) {
                throw new System.MissingMethodException("Property or field '" + n + "' not found on " + obj);
            }
        }

        // ---------------------------------------- //
        // UTILITY METHODS

        // Get type of field/property for debug purposes
        public System.Type ValueType() {
            if (propertyInfo != null) {
                return propertyInfo.PropertyType;
            } else {
                return fieldInfo.FieldType;
            }
        }

        // Check if AniValue is from given object
        public bool Is(System.Object checkObj) {
            return (obj == checkObj);
        }

        // Check if AniValue is from given object and value
        public bool Is(System.Object checkObject, string checkName) {
            return (Is(checkObject) && checkName == name);
        }

        // ---------------------------------------- //
        // GET AND SET VALUE

        // Get field or property
        public System.Object Get() {
            if (propertyInfo != null) {
                return propertyInfo.GetValue(obj, null);
            } else {
                return fieldInfo.GetValue(obj);
            }
        }

        // Set field or property
        public void Set(System.Object value) {
            try {
                if (propertyInfo != null) {
                    propertyInfo.SetValue(obj, value, null);
                } else {
                    fieldInfo.SetValue(obj, value);
                }
            } catch {
                Ani.Mate.StopAll(obj);
            }
        }
    }

    // ---------------------------------------- //
    // ANIMATOR CLASS

    public class AniMator {

        // Initial value
        public System.Object startValue;
        // End value
        public System.Object endValue;
        // Change over duration
        public System.Object change;

        // Time of animation start
        public float startTime;
        // Length of animation
        public float duration;
        // Easing class
        public AnimationEasing easing;
        // Easing type
        public EasingType easingType;
        // Animation drive
        public AnimationDrive drive;

        // Fallback with dynamic typing
        public AniMator(System.Object sta, System.Object end, System.Object chg, float dur, float delay, AnimationEasingType eas, EasingType typ, AnimationDriveType d) {
            startValue = sta;
            endValue = end;
            change = chg;
            Setup(dur, delay, eas, typ, d);
        }

        // Create Animator
        private void Setup(float dur, float delay, AnimationEasingType eas, EasingType typ, AnimationDriveType d) {
            startTime = Time.time + delay;
            duration = dur;
            easingType = typ;
            easing = new AnimationEasing(eas);
            drive = new AnimationDrive(d);
        }

        // Get easing with correct type
        public float GetEasing(float time) {
            if (easingType == EasingType.In) {
                return easing.In(time);
            } else if (easingType == EasingType.Out) {
                return easing.Out(time);
            } else if (easingType == EasingType.InOut) {
                return easing.InOut(time);
            }
            return 0;
        }

        // Get current animation position (from 0 to 1)
        public float GetPosition() {
            return Mathf.Clamp01((Time.time - startTime) / duration);
        }

        // Check if animation is running
        public bool Running() {
            return startTime < Time.time;
        }

        // Check if animation is finished
        public bool Finished() {
            return (startTime + duration) < Time.time;
        }

        // Get value for custom position
        public System.Object GetValue(float easPos) {
            // Use drive to calculate value
            return drive.Animate(startValue, endValue, change, easPos * duration, duration);
        }

        // Get current animation value
        public System.Object GetValue() {
            float easPos = GetEasing(GetPosition());
            return GetValue(easPos);
        }
    }

    // ---------------------------------------- //
    // ANIMATION DRIVES

    public enum AnimationDriveType {
        Regular,
        Slerp,
        Lerp
    }

    public class AnimationDrive {
        private AnimationDriveType type = AnimationDriveType.Regular;

        public AnimationDrive(AnimationDriveType type) {
            this.type = type;
        }

        public System.Object Animate(System.Object start, System.Object end, System.Object diff, float time, float duration) {
            switch (type) {
                case AnimationDriveType.Regular:
                    return Animate_Regular(start, end, diff, time, duration);
                case AnimationDriveType.Slerp:
                    return Animate_Slerp(start, end, diff, time, duration);
                case AnimationDriveType.Lerp:
                    return Animate_Lerp(start, end, diff, time, duration);
            }
            return null;
        }

        public bool CalculateDiff() {
            switch (type) {
                case AnimationDriveType.Regular:
                    return true;
                case AnimationDriveType.Slerp:
                case AnimationDriveType.Lerp:
                    return false;
            }
            return true;
        }

        public System.Object Animate_Regular(System.Object start, System.Object end, System.Object diff, float time, float duration) {
            // Positon
            float easPos = time / duration;
            // Cast to known types for performance
            System.Type startType = start.GetType();

            // --- Builtin types
            if (startType != diff.GetType()) {
                return (float)start + easPos * (float)diff;
            } else if (startType == typeof(short)) {
                return (short)start + easPos * (short)diff;
            } else if (startType == typeof(int)) {
                return (int)start + easPos * (int)diff;
            } else if (startType == typeof(long)) {
                return (long)start + easPos * (long)diff;
            } else if (startType == typeof(float)) {
                return (float)start + easPos * (float)diff;
            } else if (startType == typeof(double)) {
                return (double)start + easPos * (double)diff;
            } else if (startType == typeof(decimal)) {
                return (decimal)start + (decimal)easPos * (decimal)diff;
            // --- Unity types
            } else if (startType == typeof(Vector2)) {
                return (Vector2)start + easPos * (Vector2)diff;
            } else if (startType == typeof(Vector3)) {
                return (Vector3)start + easPos * (Vector3)diff;
            } else if (startType == typeof(Vector4)) {
                return (Vector4)start + easPos * (Vector4)diff;
            } else if (startType == typeof(Color)) {
                return (Color)start + easPos * (Color)diff;
            // --- Fallback
            } else {
                return ((float)start + easPos * (float)diff);
            }
        }

        public System.Object Animate_Slerp(System.Object start, System.Object end, System.Object diff, float time, float duration) {
            return Quaternion.Slerp((Quaternion)start, (Quaternion)end, (time / duration));
        }

        public System.Object Animate_Lerp(System.Object start, System.Object end, System.Object diff, float time, float duration) {
            return Vector3.Lerp((Vector3)start, (Vector3)end, (time / duration));
        }
    }

    // ---------------------------------------- //
    // EASING FUNCTIONS

    public enum EasingType {
        In,
        Out,
        InOut
    }

    public enum AnimationEasingType {
        Linear,
        Quad,
        Cube,
        Quart,
        Quint,
        Sine,
        Expo,
        Circ,
        Back,
        Bounce,
        Elastic,
        Spring
    }

    public class AnimationEasing {
        private AnimationEasingType type = AnimationEasingType.Linear;
        private float s = 1.70158f;
        private float s2 = 1.70158f * 1.525f;

        public AnimationEasing(AnimationEasingType type) {
            this.type = type;
        }

        public float In(float time) {
            switch (type) {
                case AnimationEasingType.Linear:
                    return time;
                case AnimationEasingType.Quad:
                    return (time * time);
                case AnimationEasingType.Cube:
                    return (time * time * time);
                case AnimationEasingType.Quart:
                    return Mathf.Pow(time, 4f);
                case AnimationEasingType.Quint:
                    return Mathf.Pow(time, 5f);
                case AnimationEasingType.Sine:
                    return Mathf.Sin((time - 1f) * (Mathf.PI / 2f)) + 1f;
                case AnimationEasingType.Expo:
                    return Mathf.Pow(2f, 10f * (time - 1f));
                case AnimationEasingType.Circ:
                    return (-1f * Mathf.Sqrt(1f - time * time) + 1f);
                case AnimationEasingType.Back:
                    return time * time * ((s + 1f) * time - s);
                case AnimationEasingType.Bounce:
                    return 1f - Out(1f - time);
                case AnimationEasingType.Elastic:
                    return EasingHelper.Elastic(time, EasingType.In);
                case AnimationEasingType.Spring:
                    return EasingHelper.Spring(time);
            }
            return time;
        }

        public float Out(float time) {
            switch (type) {
                case AnimationEasingType.Linear:
                    return time;
                case AnimationEasingType.Quad:
                    return (time * (time - 2f) * -1f);
                case AnimationEasingType.Cube:
                    return (Mathf.Pow(time - 1f, 3f) + 1f);
                case AnimationEasingType.Quart:
                    return (Mathf.Pow(time - 1f, 4f) - 1f) * -1f;
                case AnimationEasingType.Quint:
                    return (Mathf.Pow(time - 1f, 5f) + 1f);
                case AnimationEasingType.Sine:
                    return Mathf.Sin(time * (Mathf.PI / 2f));
                case AnimationEasingType.Expo:
                    return (-1f * Mathf.Pow(2f, -10f * time) + 1f);
                case AnimationEasingType.Circ:
                    return Mathf.Sqrt(1f - Mathf.Pow(time - 1f, 2f));
                case AnimationEasingType.Back:
                    time = time - 1f;
                    return (time * time * ((s + 1f) * time + s) + 1f);
                case AnimationEasingType.Bounce: {
                    if (time < (1f / 2.75f)) {
                        return (7.5625f * time * time);
                    } else if (time < (2f / 2.75f)) {
                        time -= (1.5f / 2.75f);
                        return (7.5625f * time * time + 0.75f);
                    } else if (time < (2.5f / 2.75f)) {
                        time -= (2.25f / 2.75f);
                        return (7.5625f * time * time + 0.9375f);
                    } else {
                        time -= (2.625f / 2.75f);
                        return (7.5625f * time * time + 0.984375f);
                    }
                }
                case AnimationEasingType.Elastic:
                    return EasingHelper.Elastic(time, EasingType.Out);
                case AnimationEasingType.Spring:
                    return EasingHelper.Spring(time);
            }
            return time;
        }

        public float InOut(float time) {
            switch (type) {
                case AnimationEasingType.Linear:
                    return time;
                case AnimationEasingType.Quad:
                case AnimationEasingType.Cube:
                case AnimationEasingType.Quart:
                case AnimationEasingType.Quint:
                case AnimationEasingType.Sine:
                case AnimationEasingType.Expo:
                case AnimationEasingType.Circ:
                case AnimationEasingType.Bounce:
                case AnimationEasingType.Elastic:
                    return EasingHelper.ElasticInOut(this, time);
                case AnimationEasingType.Back: {
                    time *= 2f;
                    if (time < 1f) {
                        return 0.5f * (time * time * ((s2 + 1f) * time - s2));
                    } else {
                        time -= 2f;
                        return 0.5f * (time * time * ((s2 + 1f) * time + s2) + 2f);
                    }
                }
                case AnimationEasingType.Spring:
                    return EasingHelper.Spring(time);
            }
            return time;
        }
    }

    public class EasingHelper {
        static private float p = 0.3f;
        static private float a = 1f;

        static public float Elastic(float time, EasingType dir) {
            float s;
            if (time == 0 ||
                time == 1f) {
                return time;
            }

            if (a < 1f) {
                s = p / 4f;
            } else {
                s = p / (2f * Mathf.PI) * Mathf.Asin(1f / a);
            }

            if (dir == EasingType.In) {
                time -= 1f;
                return -(a * Mathf.Pow(2f, 10f * time)) * Mathf.Sin((time - s) * (2f * Mathf.PI) / p);
            } else {
                return a * Mathf.Pow(2f, -10f * time) * Mathf.Sin((time - s) * (2f * Mathf.PI) / p) + 1f;
            }
        }

        static public float ElasticInOut(AnimationEasing easing, float time) {
            if (time <= 0.5f) {
                return easing.In(time * 2f) / 2f;
            } else {
                return (easing.Out((time - 0.5f) * 2f) / 2f) + 0.5f;
            }
        }

        static public float Spring(float time) {
            time = Mathf.Clamp01(time);
            time = (Mathf.Sin(time * Mathf.PI * (0.2f + 2.5f * time * time * time)) *
                Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
            return time;
        }

    }

}
