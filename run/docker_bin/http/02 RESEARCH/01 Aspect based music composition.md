# Aspect based music composition

## Summary

> Aspect-based composition decomposes a musical part into three independent components — harmonic, melodic, and rhythmic — each represented and edited separately. The harmonic aspect defines the pitch, the melodic aspect describes movement across possible piches, and the rhythmic aspect provides the temporal structure. Combining these three aspects produces a complete instrumental part, while keeping the originals intact — allowing free recombination and experimentation across different harmonic, melodic, and rhythmic configurations.

## Introduction

Traditionally, a musical composition is defined as a collection of instrumental parts, where each instrument is assigned a set of events such as the onset and ending of a sound, together with its pitch and, optionally, additional information about articulation. In this framework, an instrumental part may be represented either as standard notation on a musical staff or as a piano roll, where notes are shown as rectangles arranged along time and pitch axes. This is a complete and useful way of representing musical material, but it is certainly not the only possible perspective.

The aspect-based composition model proposes a decomposition of instrumental parts into separate and independent components such as the harmonic aspect, the melodic aspect, and the rhythmic aspect. Together, these three aspects form a complete description of a given instrumental part within a musical work. Each of these aspects can be represented and edited independently in a graphical form, potentially opening up new and interesting possibilities for musical composition.

Let us consider them one by one.

## Harmonic aspect

The harmonic aspect defines the harmony of a given instrumental part, or possibly of a selected fragment of that part. Canonically, a shared harmonic structure would normally be defined at the level of the entire composition. However, introducing a harmonic aspect already at the level of an individual instrumental part—or even a fragment of such a part—may lead to musically or at least artistically interesting effects. It is also a useful generalization that may prove valuable in specific cases.

In particular, within the canon of traditional music, the harmonic content of a given instrumental part or fragment may be understood as a subset of the set of pitches defined by the harmony of the entire piece. This seemingly unusual approach may appear to make little sense when considered in isolation from the melodic aspect. However, once the existence and function of the melodic aspect are taken into account, it becomes much easier to imagine situations in which such a construction is meaningful.

## Melodic aspect

The melodic aspect determines how the sounds of a given instrumental part move across the pitch material defined by the harmonic aspect. In the melodic aspect, time is not represented in full; it is reduced to the ordering of successive sounds in a certain sequence. The durations of individual sounds and the pauses between them are not specified here.

The melodic aspect can therefore be understood as a trajectory defined by the succession of sounds together with an abstract scale describing upward or downward motion in pitch. In the case of monophonic instruments, this scale can be mapped directly onto specific pitches derived from the harmonic aspect. In the case of chordal playing, it becomes necessary to define the transposition of the chord specified in the harmonic aspect. Another possibility is to move within the set of chords directly defined by the harmonic aspect.

## Rhythmic aspect

The rhythmic aspect concerns the precise temporal structure of a given instrumental part or its fragment. It defines the start and end times of individual sounds or chords. The temporal scale is relative and does not refer directly to absolute units such as seconds. Instead, it operates on an abstraction which requires a specific tempo in order to be translated into absolute time units. The tempo itself may, of course, vary if needed.

## Combining the aspects

In order to create a complete instrumental part, or a fragment thereof, all of the above aspects are combined. The result is a coherent instruction specifying when individual notes begin and end, and at what pitch they occur. Importantly, the operation of combining the aspects does not destroy the original, separate aspects representations. As a result, the combination process can be repeated for arbitrary combinations of harmonic, melodic, and rhythmic aspects.

## Innovative character of the method

>This method is innovative in at least two ways:
>
>- Expressing composition in the language of separate harmonic, melodic, and rhythmic aspects may create a new compositional perspective and thus contribute to the emergence of new and interesting musical works.
>- The possibility of testing, experimenting with, and recombining different harmonic, melodic, and rhythmic aspects may, through exploration and experimentation, lead to the creation of new and compelling musical compositions.

## Implementation

The idea of _Aspect based music composition_ has been implemented as a stand alone public domain music composition software [-composer](http://tereszczuk.com/-composer).