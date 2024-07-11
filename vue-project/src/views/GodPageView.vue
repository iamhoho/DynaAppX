<script setup>
import LookUpControl from '../components/control/LookUpControl.vue';
import StringControl from '../components/control/StringControl.vue';
import BoolControl from '../components/control/BoolControl.vue';
import NumberControl from '../components/control/NumberControl.vue';
import DateControl from '../components/control/DateControl.vue';
import EntityControl from '../components/control/EntityControl.vue';
import { daxHelper } from '../daxHelper.js'
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'

const selectedEntity = ref(null);
const selectedRecord = ref(null);
const entityDefinition = ref(null);
const attributes = ref([]);
const hiddenAttributes = ['versionnumber', 'utcconversiontimezonecode', 'timezoneruleversionnumber', 'owninguser', 'owningteam', 'owningbusinessunit', 'overriddencreatedon', 'modifiedonbehalfby', 'importsequencenumber', 'createdonbehalfby'];
const disableAttributes = ['statuscode', 'statecode', 'ownerid', 'modifiedon', 'modifiedby', 'createdon', 'createdby'];
const inputData = ref({});

function loadData(entityInfo, recordLookUp) {

}

function setAttributes() {
    attributes.value = [];
    if (entityDefinition.value?.Attributes) {
        attributes.value = entityDefinition.value.Attributes.filter((x) => {
            return x.AttributeOf == null && !x.IsPrimaryId && !hiddenAttributes.includes(x.LogicalName);
        })
    }
}

watch(() => selectedEntity.value, (newValue, oldValue) => {
    selectedRecord.value = null;
    entityDefinition.value = daxHelper.getEntityDefinition(selectedEntity.value?.MetadataId);
    setAttributes();
})

watch(() => selectedRecord.value, (newValue, oldValue) => {
    loadData(selectedEntity.value, selectedRecord.value);
})

</script>

<template>
    <div style="display: flex;flex-direction: row;justify-content: center;">
        <EntityControl attName="Entity" :required="true" ref="selectEntity" :disabled="false" v-model="selectedEntity">
        </EntityControl>
        <LookUpControl attName="Record" :logicalName="selectedEntity?.LogicalName" :required="true"
            :disabled="selectedEntity == null" v-model="selectedRecord"></LookUpControl>
    </div>

    <div style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center;" v-if="selectedRecord != null">
        <div v-for="attribute in attributes">
            <div v-if="attribute.AttributeType == 'Boolean'">
                <BoolControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :attName="attribute.DisplayName.UserLocalizedLabel.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"></BoolControl>
            </div>
            <div
                v-else-if="attribute.AttributeType == 'Double' || attribute.AttributeType == 'Money' || attribute.AttributeType == 'Decimal' || attribute.AttributeType == 'Integer'">
                <NumberControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :attName="attribute.DisplayName.UserLocalizedLabel.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)">
                </NumberControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'DateTime'">
                <DateControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :attName="attribute.DisplayName.UserLocalizedLabel.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"></DateControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'String' || attribute.AttributeType == 'Memo'">
                <StringControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :attName="attribute.DisplayName.UserLocalizedLabel.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"></StringControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'Lookup'">
                <LookUpControl :attName="attribute.DisplayName.UserLocalizedLabel.Label" :logicalName=attribute.Targets
                    :required="false" :disabled="disableAttributes.includes(attribute.LogicalName)"
                    v-model="inputData[attribute.LogicalName]"></LookUpControl>
            </div>
            <div v-else>
                <StringControl v-model="attribute.DisplayName.UserLocalizedLabel.Label" :required="false"
                    :attName="attribute.DisplayName.UserLocalizedLabel.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"></StringControl>
                <!-- <div>Unsupported Attribute:</div>
                <div>Attribute DisplayName:{{ attribute.DisplayName.UserLocalizedLabel?.Label }}</div>
                <div>Attribute LogicalName:{{ attribute.LogicalName }}</div>
                <div>Attribute AttributeType:{{ attribute.AttributeType }}</div>
                <div>Attribute RequiredLevel:{{ attribute.RequiredLevel.Value }}</div> -->
            </div>
        </div>
    </div>
</template>
