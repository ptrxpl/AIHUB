import time
start_time = time.time()
n = 2

for x in range(0, n):
    print(f"Current x iteration: {x + 1} / {n}")
    print("Going to sleep for 5 seconds...")
    time.sleep(5)
    print("I'm awake!")
    
print(f"Program took: {time.time() - start_time} seconds")