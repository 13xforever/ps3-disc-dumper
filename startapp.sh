#!/bin/sh

echo "CD to app"

cd /app

echo "Try to mount cd drive"

umount /dev/sr0

echo "Checking optical drive"

./check-optical-drive.sh

#Doing dbus stuff

#pkill -9 dbus-daemon || true

#rm -f /var/run/dbus/dbus.pid

#rm -rf /var/run/dbus/system_bus_socket

#dbus-daemon --system --nofork --nopidfile --address=unix:path=/var/run/dbus/system_bus_socket &

echo "Starting ps3-disc-dumper"

./ps3-disc-dumper

#For testing

while true;do sleep 60;done