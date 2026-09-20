# Weather Owner Stack

A read-only runnable consumer for the ownership contract behind the accepted Rain / Day stack.

It reads the public TaintedWeatherApi snapshot and emits three separate observations:

~~~text
weather truth
→ state

sky consumer
→ requested environment + time bucket

water consumer
→ requested water environment
~~~

The example deliberately performs **no sky or water mutation**. That is the point: the semantic owner publishes state; each real presentation consumer owns its own output.

## Build

~~~powershell
dotnet build .\WeatherOwnerStack.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Install beside a current Tainted Weather setup. The example has no compile-time dependency on TaintedWeather.dll because it consumes the public snapshot through a narrow reflection boundary.

## Expected logs

When the snapshot changes, expect separate owner rows similar to:

~~~text
owner=weather-truth state=Rain; mutationByThisExample=false
consumer=sky requestedEnvironment=...; timeBucket=Day; skyMutationByThisExample=false
consumer=water requestedEnvironment=GreenShallows; externalConsumerRequested=True; waterMutationByThisExample=false
~~~

## Boundary

The accepted live case proved actual Tainted Skybox and Immersive Water consumers. This teaching project demonstrates the reusable ownership contract without pretending to replace those production consumers.

Guide: [Build a bounded weather consumer stack](../../../../guides/tasks/rendering/build-a-bounded-weather-consumer-stack.md)  
Evidence: [Rain / Day owner-stack validation](../../../../research/case-studies/weather/rain-day-owner-stack.md)
