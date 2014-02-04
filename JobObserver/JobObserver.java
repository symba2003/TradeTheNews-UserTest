package com.exoriens.intp.application.job;

import org.apache.log4j.Logger;

import java.util.Collection;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

/**
 * Created by i005144 on 1/16/14.
 */
public class JobObserver {

    private final Logger log = Logger.getLogger(JobObserver.class);
    private final boolean isTraceEnabled = log.isTraceEnabled();
    private final String prefix;
    private final ReentrantLock jobGuardLock = new ReentrantLock();
    private final Condition deltasExists = jobGuardLock.newCondition();
    private boolean isInitialTriggered = false;
    private int deltaCounter;

    public JobObserver(String prefix) {
        this.prefix = prefix;
    }

    public boolean tryStartInitial() {
        boolean startAllowed = false;
        try {
            jobGuardLock.lock();
            startAllowed = !isInitialTriggered;
            if (startAllowed)
                isInitialTriggered = true;
        } finally {
            jobGuardLock.unlock();
        }
        if (isTraceEnabled) {
            if (startAllowed)
                log.trace(prefix + " Initial triggered");
            else
                log.trace(prefix + " Initial already running, skipping new one...");
        }
        return startAllowed;
    }

    public void setInitialFinished(String uuid) {
        boolean stopped = false;
        try {
            jobGuardLock.lock();
            stopped = isInitialTriggered;
            if (stopped)
                isInitialTriggered = false;
        } finally {
            jobGuardLock.unlock();
        }
        if (!stopped)
            log.error(prefix + " Cannot finish Initial job, Initial is not running!");
        else if (isTraceEnabled)
            log.trace(prefix + " Initial finished: " + uuid);
    }

    public boolean tryStartDelta() {
        boolean startAllowed = false;
        try {
            jobGuardLock.lock();
            startAllowed = !isInitialTriggered;
            if (startAllowed)
                deltaCounter++;
        } finally {
            jobGuardLock.unlock();
        }
        if (isTraceEnabled) {
            if (startAllowed)
                log.trace(prefix + " Delta triggered");
            else
                log.trace(prefix + " Delta skipping - Initial was triggered but was not finished");
        }
        return startAllowed;
    }

    public void setDeltaFinished(Collection<String> uuids) {
        try {
            jobGuardLock.lock();
            deltaCounter -= uuids.size();
            deltasExists.signal();
        } finally {
            jobGuardLock.unlock();
        }
        if (isTraceEnabled)
            log.trace(prefix + " Delta finished: " + uuids);
    }

    public void awaitForDeltasCompletion() throws InterruptedException {
        log.debug(prefix + " Initial job, awaiting deltas completion...");
        while (true) {
            try {
                jobGuardLock.lock();
                if (deltaCounter == 0) {
                    if (isTraceEnabled)
                        log.trace(prefix + " Initial job, no deltas are running.");
                    return;
                } else {
                    if (isTraceEnabled)
                        log.trace(String.format("%s Initial job, [%d] deltas are running, awaiting for completion.", prefix, deltaCounter));
                    deltasExists.await();
                }
            } finally {
                jobGuardLock.unlock();
            }
        }
    }
}
