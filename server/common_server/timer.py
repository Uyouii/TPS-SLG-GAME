# -*- coding: GBK -*-

import heapq
import time


class CallLater(object):
    """Calls a function at a later time."""

    def __init__(self, seconds, target, *args, **kwargs):
        super(CallLater, self).__init__()

        self._delay = seconds
        self._target = target
        self._args = args
        self._kwargs = kwargs

        self.cancelled = False
        self.timeout = time.time() + self._delay

    def __le__(self, other):
        return self.timeout <= other.timeout

    def call(self):
        try:
            self._target(*self._args, **self._kwargs)
        except (KeyboardInterrupt, SystemExit):
            raise

        return False

    def cancel(self):
        self.cancelled = True


class CallEvery(CallLater):
    """Calls a function every x seconds. """

    def call(self):
        try:
            self._target(*self._args, **self._kwargs)
        except (KeyboardInterrupt, SystemExit):
            raise

        self.timeout = time.time() + self._delay

        return True


class TimerManager(object):
    tasks = []
    cancelled_num = 0

    @staticmethod
    def addTimer(delay, func, *args, **kwargs):
        timer = CallLater(delay, func, *args, **kwargs)

        heapq.heappush(TimerManager.tasks, timer)

        return timer

    @staticmethod
    def addRepeatTimer(delay, func, *args, **kwargs):
        timer = CallEvery(delay, func, *args, **kwargs)

        heapq.heappush(TimerManager.tasks, timer)

        return timer

    @staticmethod
    def scheduler():
        now = time.time()
        while TimerManager.tasks and now >= TimerManager.tasks[0].timeout:
            call = heapq.heappop(TimerManager.tasks)
            if call.cancelled:
                TimerManager.cancelled_num -= 1
                continue

            try:
                repeated = call.call()
            except (KeyboardInterrupt, SystemExit):
                raise

            if repeated:
                heapq.heappush(TimerManager.tasks, call)

    @staticmethod
    def cancel(timer):
        if not timer in TimerManager.tasks:
            return

        timer.cancel()
        TimerManager.cancelled_num += 1

        if float(TimerManager.cancelled_num) / len(TimerManager.tasks) > 0.25:
            TimerManager.removeCancelledTasks()

        return

    @staticmethod
    def removeCancelledTasks():
        print 'remove cancelled tasks'
        tmp_tasks = []
        for t in TimerManager.tasks:
            if not t.cancelled:
                tmp_tasks.append(t)

        TimerManager.tasks = tmp_tasks
        heapq.heapify(TimerManager.tasks)

        TimerManager.cancelled_num = 0

        return
