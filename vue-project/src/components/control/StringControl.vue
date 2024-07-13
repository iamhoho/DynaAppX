<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    lableName: {
    },
    disabled: {
        required: true
    },
    attrabuteName: {
    }
}
)

const value = ref(props.modelValue)

const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    emit('update:modelValue', newValue);
})

</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLable">
            <p>{{ lableName }}</p>
            <p v-if="attrabuteName">{{ attrabuteName }}</p>
        </div>
        <el-input class="controlItemTextarea" v-model="value" placeholder="Please input" type="textarea"
            :disabled=disabled />
        <el-icon style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && (modelValue == null || modelValue == '')" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
</template>