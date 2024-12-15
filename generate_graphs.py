import pandas as pd
import matplotlib.pyplot as plt
import json

# Загрузка данных
with open('results.json') as f:
    data = json.load(f)

# Парсинг данных
requests = [entry for entry in data['metrics']['http_reqs']['values']]

# Построение графика Latency
plt.figure(figsize=(10, 5))
plt.plot([r['time'] for r in requests], [r['value'] for r in requests], label='Latency', color='blue')
plt.title('Latency over Time')
plt.xlabel('Time (s)')
plt.ylabel('Latency (ms)')
plt.legend()
plt.grid(True)
plt.savefig('latency_graph.png')  # Сохраняем график в файл
plt.close()  # Закрываем график

# Построение графика Throughput
throughput = [entry['value'] for entry in data['metrics']['http_req_duration']['values']]
plt.figure(figsize=(10, 5))
plt.plot(range(len(throughput)), throughput, label='Throughput', color='green')
plt.title('Throughput over Time')
plt.xlabel('Time (s)')
plt.ylabel('Requests per Second')
plt.legend()
plt.grid(True)
plt.savefig('throughput_graph.png')  # Сохраняем график в файл
plt.close()  # Закрываем график
