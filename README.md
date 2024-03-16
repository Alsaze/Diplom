I'm developing a college degree. I'm developing Unity polygon and drone management
Python Script for Unity
```
import socket
import cv2
import numpy as np
from tkinter import *
from PIL import Image, ImageTk
import pygame
import threading
from time import sleep
import websocket

ws = websocket.WebSocketApp("ws://127.0.0.1:8080", on_open=lambda _: print("Connection opened"))


def sendJoystickInput():
    pygame.init()
    pygame.joystick.init()
    joystick = pygame.joystick.Joystick(0)
    joystick.init()

    while True:
        pygame.event.get()
        left_axis = [joystick.get_axis(0), joystick.get_axis(1)]
        right_axis = [joystick.get_axis(2), joystick.get_axis(3)]

        if ws.sock.connected:
            ws.send(';'.join([str(a) for a in left_axis + right_axis]))

        sleep(1 / 10)


def receiveAndDisplayImage():
    UDP_IP = "127.0.0.1"
    UDP_PORT = 8080

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((UDP_IP, UDP_PORT))

    root = Tk()
    canvas = Canvas(root, width=640, height=480)
    canvas.pack()

    while True:
        data, addr = sock.recvfrom(65507)
        img = cv2.imdecode(np.frombuffer(data, dtype=np.uint8), cv2.IMREAD_COLOR)
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        img = Image.fromarray(img)
        img_tk = ImageTk.PhotoImage(image=img)

        canvas.create_image(0, 0, anchor=NW, image=img_tk)
        root.update()


# Создаем и запускаем потоки для отправки данных с джойстика и приема/отображения изображения
joystick_thread = threading.Thread(target=sendJoystickInput)
image_thread = threading.Thread(target=receiveAndDisplayImage)

joystick_thread.start()
image_thread.start()
ws.run_forever()
```
