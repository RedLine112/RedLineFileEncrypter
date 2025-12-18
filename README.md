# RedLine Archiver

Hi there! Welcome to my file encryption project.

I built RedLine Archiver because I wanted a tool that not only secures my files but also looks cool while doing it. It is a C# application that lets you lock your folders with a password, using a dark, modern interface.

## What does it do?

It takes any file or folder you drag into it, squeezes it to make it smaller (compression), and then encrypts it so nobody else can read it without your password.

## Why is it special?

Most programs just use standard codes. I included those too, but I also created my own custom algorithm called **IronWall**.

- **IronWall Algorithm:** This is my personal touch. I designed a logic that shuffles the bits, adds random salt, and changes the key dynamically. It creates a chaotic mess that is super hard to reverse without the password.
- **Classic Options:** If you prefer the industry standards, you can still use AES-256 (Safest), TripleDES, or RC2.
- **3 Languages:** I added support for English, Turkish, and Russian. You can switch instantly by clicking the language code at the top. It can't be said that it works perfectly, but at least it translates the necessary parts.

## How to use it

1. Open the program.
2. Drag and drop a file or folder into the box.
3. Choose an encryption method (try "RedLine XOR" to see my custom code in action).
4. Set a password.
5. Click the LOCK button.

To open your files again, just drag the locked file back in, select true algorithm, type the password, and click UNLOCK.

## A small note

I created this project to learn more about cryptography and software architecture. While my IronWall algorithm is very complex and strong, if you are protecting extremely sensitive data (like bank details), using the AES-256 option might be the safest bet.

Hope you like it!

<img width="486" height="534" alt="image" src="https://github.com/user-attachments/assets/6a0a50a6-5720-4b22-8636-b061022ef54f" />
