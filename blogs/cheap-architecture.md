![](https://cdn-images-1.medium.com/max/2000/0*71bax7oD7_lYDiyC.jpg)

### How abstract is too abstract?

Enterprise software development tends to be riddled with abstract patterns and concepts. You can master the pillars of Object Oriented Programming — polymorphism, inheritance, encapsulation, and abstraction. You can learn about [Design Patterns](https://exceptionnotfound.net/tag/design-patterns/) such as the Strategy pattern and the Decorator pattern. You can apply [SOLID principles](https://medium.com/@dhkelmendi/solid-principles-made-easy-67b1246bcdf) so that your code is loosely coupled, highly cohesive, and as extensible as possible.

But why? What benefits do the businesses actually receive when we apply all of these patterns and concepts to their software?

It’s not that these things don’t supply any value, because they certainly can and do when done right. However, as a developer it’s far too easy to become so involved with learning and applying these abstract concepts that the reason *why* they’re being applied gets lost. And when that happens, the actual value in learning and applying these concepts can also be lost.

In order to realize why we do these things, we’ll need to take a step back and find some common ground between some of the concepts.

---

### Recognizing a pattern

A couple of years ago, I read Domain Driven Design by Eric Evans (the [blue book](https://amzn.to/2XEx3LW)). Being relatively new to software development, reading the concepts in this book and seeing how they applied to real world software stood out to me. I then began seeking out ways to design an elegant domain model. Trying to force these abstract concepts into existing enterprise applications is not easy, and you can easily end up with scattered remnants of Aggregate Roots and Value Objects that don’t fit in with anything else.

More recently, I’ve read [Clean Architecture](https://amzn.to/32mxiP7) by Robert C. Martin. While reading this, it was almost impossible to not recognize the similarities of the concepts with DDD.

From crafting a well encapsulated model that enforces the invariant constraints of the business, to defining separate layers of code such that high-level enterprise policies are not impacted by changes to low-level application details, both of these ideas ultimately focus on one thing: isolating the code that’s important to the business from the code that’s not (determining what code is “important” is the hard part).

---

### Back to business

With that similarity uncovered, it allows us to see some general value behind all of these concepts.

When you can have a system where the important code is truly encapsulated from the rest of the system, a number of benefits will unfold:

1. The critical components of the business can now be easily tested.

1. You can make cross-cutting changes to the core behavior of the system quickly and confidently.

1. You can change underlying details (infrastructure, frameworks, etc.) of the application without affecting the behavior of the system.

These are all wonderful, but there is yet one underlying benefit that they all provide for the business: They make the software cheap.

Cheap to test. Cheap to extend. Cheap to optimize.

---

### Win, Win

When looking for new patterns to learn and concepts to apply in enterprise software, it helps to keep this in mind. Because if you’re able to learn and build great software while simultaneously creating future business value, then everybody wins.
