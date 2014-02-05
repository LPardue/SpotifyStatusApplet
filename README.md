SpotifyStatusApplet
===================

An LCD Applet for the Logitech Gaming keyboard family (G510, G13, G15 etc) that displays current track information.

Running
--------------------------------------

SpotifyStatusApplet is dependent on:

- An instance of Spotify running (of course!). The applet utilises the Spotify service SpotifyWebHelper.exe to access information, further details are available at [Spotify Local API](https://code.google.com/p/spotify-local-api/)
- A compatible [Logitech Gaming Keyboard](http://gaming.logitech.com/en-gb/gaming-keyboards) with LCD and official Logitech drivers / software installed. The software has only been tested on a G510 to date but support for all LCD equipped devices is expected.
- Windows and .Net. Tested on Windows 7 64-bit and .Net Framework 4.

Environment and Building
--------------------------------------

SpotifyStatusApplet is written in C# and has been developed using Visual Studio Express 2012 for Windows Desktop. A solution file is provided to gather all dependencies, it is anticipated this could be backported to support earlier versions of Visual Studio.

Dependencies
--------------------------------------

SpotifyStatusApplet builds upon the great efforts of other developers/projects! The following software components are incorporated into the source tree to aid distribution

- [Spotify Local API](https://code.google.com/p/spotify-local-api/) - A very simple and small library that allows .NET developers to get track information, (un)pause spotify, play tracks, get cover art and more! 
- [GammaJul LgLcd](http://gjlglcd.codeplex.com/) - A .NET wrapper around the Logitech SDK for G15/G19 keyboard screens. Supports raw byte sending, GDI+ drawing and rendering WPF elements onto the screen.
- [Json.net](http://json.codeplex.com/) - Json.NET is a popular high-performance JSON framework for .NET.
