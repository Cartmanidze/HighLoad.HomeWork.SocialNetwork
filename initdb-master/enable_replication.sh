#!/bin/bash
set -e

echo "host replication postgres 0.0.0.0/0 trust" >> /var/lib/postgresql/data/pg_hba.conf
echo "wal_level = replica" >> /var/lib/postgresql/data/postgresql.conf
echo "max_wal_senders = 10" >> /var/lib/postgresql/data/postgresql.conf
echo "max_replication_slots = 10" >> /var/lib/postgresql/data/postgresql.conf
echo "synchronous_standby_names = ''" >> /var/lib/postgresql/data/postgresql.conf
